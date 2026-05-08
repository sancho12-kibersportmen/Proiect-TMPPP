using FlightBooking.Interfaces;

namespace FlightBooking.Structural.Bridge
{
    // ── Implementori concreti (ConcreteImplementor) ──────────────────

    // Renderer 1 – Plain Text (pentru SMS / consola)
    public class PlainTextRenderer : IMessageRenderer
    {
        public string RendererName => "PlainText";
        public string RenderTitle(string title)        => $"=== {title.ToUpper()} ===";
        public string RenderBody(string body)          => body;
        public string RenderField(string label, string value) => $"  {label}: {value}";
        public string RenderFooter(string footer)      => $"---\n{footer}";
    }

    // Renderer 2 – HTML (pentru Email)
    public class HtmlRenderer : IMessageRenderer
    {
        public string RendererName => "HTML";
        public string RenderTitle(string title)
            => $"<h2 style='color:#003580'>{title}</h2>";
        public string RenderBody(string body)
            => $"<p>{body}</p>";
        public string RenderField(string label, string value)
            => $"<p><b>{label}:</b> {value}</p>";
        public string RenderFooter(string footer)
            => $"<hr/><small>{footer}</small>";
    }

    // Renderer 3 – Markdown (pentru Slack / Teams)
    public class MarkdownRenderer : IMessageRenderer
    {
        public string RendererName => "Markdown";
        public string RenderTitle(string title)        => $"## {title}";
        public string RenderBody(string body)          => body;
        public string RenderField(string label, string value) => $"**{label}:** {value}";
        public string RenderFooter(string footer)      => $"\n---\n_{footer}_";
    }
}
