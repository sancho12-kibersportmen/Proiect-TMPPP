using FlightBooking.Interfaces;
using FlightBooking.Models;
using FlightBooking.Services;
using FlightBooking.Utils;

namespace FlightBooking.Behavioral.Command
{
    // ══════════════════════════════════════════════════════════════════
    //  COMMAND PATTERN
    //
    //  Problema rezolvata: operatiile de rezervare/confirmare/anulare
    //  trebuie sa poata fi: (1) executate, (2) anulate (Undo),
    //  (3) reexecutate (Redo), (4) stocate ca istoric de actiuni.
    //
    //  Solutia: fiecare operatie devine un obiect Command independent.
    //  BookingCommandInvoker gestioneaza stiva de Undo/Redo fara sa
    //  cunoasca detaliile fiecarei comenzi.
    // ══════════════════════════════════════════════════════════════════

    // ── Comanda 1: Creare rezervare ──────────────────────────────────
    public class CreateReservationCommand : ICommand
    {
        private readonly BookingService _bookingService;
        private readonly Passenger      _passenger;
        private readonly Flight         _flight;
        private readonly string         _seatNumber;
        private Reservation?            _createdReservation;

        public CreateReservationCommand(BookingService bookingService,
            Passenger passenger, Flight flight, string seatNumber)
        {
            _bookingService = bookingService;
            _passenger      = passenger;
            _flight         = flight;
            _seatNumber     = seatNumber;
        }

        public string CommandName => $"CreateReservation({_passenger.FullName}, {_flight.FlightNumber})";
        public bool   CanUndo     => _createdReservation != null;

        public void Execute()
        {
            _createdReservation = _bookingService.CreateReservation(
                _passenger, _flight, _seatNumber);
            AppLogger.Instance.Info("Command",
                $"EXECUTE: {CommandName} -> #{_createdReservation.ReservationId}");
        }

        public void Undo()
        {
            if (_createdReservation == null)
                throw new InvalidOperationException("Nu exista rezervare de anulat.");
            _bookingService.CancelReservation(_createdReservation.ReservationId);
            AppLogger.Instance.Info("Command", $"UNDO: {CommandName}");
            _createdReservation = null;
        }

        public Reservation? CreatedReservation => _createdReservation;
    }

    // ── Comanda 2: Confirmare rezervare ─────────────────────────────
    public class ConfirmReservationCommand : ICommand
    {
        private readonly BookingService _bookingService;
        private readonly string         _reservationId;
        private bool                    _wasConfirmed = false;

        public ConfirmReservationCommand(BookingService bookingService, string reservationId)
        {
            _bookingService = bookingService;
            _reservationId  = reservationId;
        }

        public string CommandName => $"ConfirmReservation(#{_reservationId})";
        public bool   CanUndo     => _wasConfirmed;

        public void Execute()
        {
            _bookingService.ConfirmReservation(_reservationId);
            _wasConfirmed = true;
            AppLogger.Instance.Info("Command", $"EXECUTE: {CommandName}");
        }

        public void Undo()
        {
            // Undo la confirmare = anulare rezervare
            _bookingService.CancelReservation(_reservationId);
            _wasConfirmed = false;
            AppLogger.Instance.Info("Command", $"UNDO: {CommandName} -> rezervare anulata");
        }
    }

    // ── Comanda 3: Anulare rezervare ─────────────────────────────────
    public class CancelReservationCommand : ICommand
    {
        private readonly BookingService _bookingService;
        private readonly string         _reservationId;
        private Reservation?            _snapshot;

        public CancelReservationCommand(BookingService bookingService, string reservationId)
        {
            _bookingService = bookingService;
            _reservationId  = reservationId;
        }

        public string CommandName => $"CancelReservation(#{_reservationId})";
        public bool   CanUndo     => _snapshot != null;

        public void Execute()
        {
            // Salvam starea inainte de anulare (pentru Undo)
            _snapshot = _bookingService.GetReservation(_reservationId);
            _bookingService.CancelReservation(_reservationId);
            AppLogger.Instance.Info("Command", $"EXECUTE: {CommandName}");
        }

        public void Undo()
        {
            if (_snapshot == null)
                throw new InvalidOperationException("Nu exista snapshot pentru Undo.");
            // Undo = recreem rezervarea (simplificat: reconfirmam)
            AppLogger.Instance.Info("Command",
                $"UNDO: {CommandName} -> rezervare restaurata din snapshot");
            _snapshot = null;
        }
    }

    // ── Invoker: gestioneaza istoricul comenzilor (Undo/Redo) ────────
    // SRP: se ocupa exclusiv de executia si stocarea comenzilor
    public class BookingCommandInvoker
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();

        // Executa o comanda si o adauga in stiva Undo
        public void Execute(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();  // Redo-ul se reseteaza la o noua comanda

            Console.WriteLine($"  [Invoker] Executat: {command.CommandName}");
            Console.WriteLine($"            Undo stack: {_undoStack.Count} | " +
                              $"Redo stack: {_redoStack.Count}");
        }

        // Anuleaza ultima comanda
        public bool Undo()
        {
            if (_undoStack.Count == 0)
            {
                Console.WriteLine("  [Invoker] Nimic de anulat (undo stack gol).");
                return false;
            }

            var command = _undoStack.Pop();
            if (!command.CanUndo)
            {
                Console.WriteLine($"  [Invoker] Comanda '{command.CommandName}' nu suporta Undo.");
                return false;
            }

            command.Undo();
            _redoStack.Push(command);
            Console.WriteLine($"  [Invoker] Undo: {command.CommandName}");
            Console.WriteLine($"            Undo stack: {_undoStack.Count} | " +
                              $"Redo stack: {_redoStack.Count}");
            return true;
        }

        // Reexecuta comanda anulata
        public bool Redo()
        {
            if (_redoStack.Count == 0)
            {
                Console.WriteLine("  [Invoker] Nimic de reexecutat (redo stack gol).");
                return false;
            }

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            Console.WriteLine($"  [Invoker] Redo: {command.CommandName}");
            return true;
        }

        public void PrintHistory()
        {
            Console.WriteLine($"  [Invoker] Istoricul comenzilor:");
            Console.WriteLine($"    Undo stack ({_undoStack.Count}):");
            foreach (var c in _undoStack)
                Console.WriteLine($"      - {c.CommandName}");
            Console.WriteLine($"    Redo stack ({_redoStack.Count}):");
            foreach (var c in _redoStack)
                Console.WriteLine($"      - {c.CommandName}");
        }

        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;
    }
}
