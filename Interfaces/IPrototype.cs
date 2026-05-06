namespace FlightBooking.Interfaces
{
    // ── Interfata Prototype ───────────────────────────────────────────
    // ISP: interfata specifica exclusiv operatiei de clonare
    public interface IPrototype<T>
    {
        T ShallowCopy();   // copie superficiala
        T DeepCopy();      // copie profunda
    }
}
