using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WorkForceManagerAPI.Models.DTOs.Response;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Services;

public class ReporteExportService : IReporteExportService
{
    private const string Dark = "#10151b";
    private const string Lime = "#c9f36d";
    private const string Line = "#e6eaed";

    static ReporteExportService() =>
        QuestPDF.Settings.License = LicenseType.Community;

    // ==================== Excel ====================
    public byte[] HorasExcel(IEnumerable<ReporteHorasRow> rows, DateOnly fechaInicio, DateOnly fechaFin)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Horas por empleado");
        Titulo(ws, "Reporte de horas por empleado", fechaInicio, fechaFin, 12);

        string[] headers = ["Empleado", "Área", "Cargo", "Días prog.", "Días asist.", "Horas prog.", "Horas trabajadas", "Horas extra", "Horas faltantes", "Tardanzas", "Cumplimiento %"];
        for (var i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];
        EstiloCabecera(ws.Range(3, 1, 3, headers.Length));

        var fila = 4;
        foreach (var r in rows)
        {
            ws.Cell(fila, 1).Value = r.NombreCompleto;
            ws.Cell(fila, 2).Value = r.Area;
            ws.Cell(fila, 3).Value = r.Cargo;
            ws.Cell(fila, 4).Value = r.DiasProgramados;
            ws.Cell(fila, 5).Value = r.DiasAsistidos;
            ws.Cell(fila, 6).Value = r.HorasProgramadas;
            ws.Cell(fila, 7).Value = r.HorasTrabajadas;
            ws.Cell(fila, 8).Value = r.HorasExtras;
            ws.Cell(fila, 9).Value = r.HorasFaltantes;
            ws.Cell(fila, 10).Value = r.Tardanzas;
            ws.Cell(fila, 11).Value = r.Cumplimiento;
            fila++;
        }
        if (fila > 4) ws.Range(4, 1, fila - 1, headers.Length).Style.Border.BottomBorder = XLBorderStyleValues.Hair;
        ws.Columns().AdjustToContents();
        return Guardar(wb);
    }

    public byte[] AsistenciaExcel(IEnumerable<ReporteAsistenciaRow> rows, DateOnly fechaInicio, DateOnly fechaFin)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Asistencia");
        Titulo(ws, "Reporte detallado de asistencia", fechaInicio, fechaFin, 8);

        string[] headers = ["Fecha", "Empleado", "Área", "Entrada", "Salida", "Horas", "Retraso (min)", "Estado"];
        for (var i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];
        EstiloCabecera(ws.Range(3, 1, 3, headers.Length));

        var fila = 4;
        foreach (var r in rows)
        {
            ws.Cell(fila, 1).Value = r.Fecha.ToDateTime(TimeOnly.MinValue);
            ws.Cell(fila, 1).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Cell(fila, 2).Value = r.NombreCompleto;
            ws.Cell(fila, 3).Value = r.Area;
            ws.Cell(fila, 4).Value = r.HoraEntrada.ToString("HH:mm");
            ws.Cell(fila, 5).Value = r.HoraSalida?.ToString("HH:mm") ?? "En curso";
            ws.Cell(fila, 6).Value = r.HorasTrabajadas;
            ws.Cell(fila, 7).Value = r.MinutosRetraso;
            ws.Cell(fila, 8).Value = r.Estado;
            fila++;
        }
        if (fila > 4) ws.Range(4, 1, fila - 1, headers.Length).Style.Border.BottomBorder = XLBorderStyleValues.Hair;
        ws.Columns().AdjustToContents();
        return Guardar(wb);
    }

    // ==================== PDF ====================
    public byte[] HorasPdf(IEnumerable<ReporteHorasRow> rows, DateOnly fechaInicio, DateOnly fechaFin)
    {
        var lista = rows.ToList();
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurarPagina(page, PageSizes.A4.Landscape());
                page.Header().Element(c => Encabezado(c, "Reporte de horas por empleado", fechaInicio, fechaFin));
                page.Content().PaddingTop(14).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3.4f); cols.RelativeColumn(2);
                        cols.RelativeColumn(1.1f); cols.RelativeColumn(1.1f);
                        cols.RelativeColumn(1.3f); cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.2f); cols.RelativeColumn(1.3f);
                        cols.RelativeColumn(1.1f); cols.RelativeColumn(1.4f);
                    });
                    table.Header(header =>
                    {
                        foreach (var titulo in new[] { "Empleado", "Área", "Días prog.", "Días asist.", "Horas prog.", "Horas trabaj.", "Extra", "Faltante", "Tardanzas", "Cumplim. %" })
                            header.Cell().Element(CeldaCabecera).Text(titulo);
                    });
                    foreach (var r in lista)
                    {
                        foreach (var valor in new[]
                        {
                            r.NombreCompleto, r.Area ?? "—", r.DiasProgramados.ToString(), r.DiasAsistidos.ToString(),
                            r.HorasProgramadas.ToString("0.##"), r.HorasTrabajadas.ToString("0.##"),
                            r.HorasExtras.ToString("0.##"), r.HorasFaltantes.ToString("0.##"),
                            r.Tardanzas.ToString(), r.Cumplimiento.ToString("0.##")
                        })
                            table.Cell().Element(CeldaCuerpo).Text(valor);
                    }
                });
                page.Footer().Element(PiePagina);
            });
        }).GeneratePdf();
    }

    public byte[] AsistenciaPdf(IEnumerable<ReporteAsistenciaRow> rows, DateOnly fechaInicio, DateOnly fechaFin)
    {
        var lista = rows.ToList();
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurarPagina(page, PageSizes.A4);
                page.Header().Element(c => Encabezado(c, "Reporte detallado de asistencia", fechaInicio, fechaFin));
                page.Content().PaddingTop(14).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(1.3f); cols.RelativeColumn(3.2f);
                        cols.RelativeColumn(2); cols.RelativeColumn(1.2f);
                        cols.RelativeColumn(1.2f); cols.RelativeColumn(1.1f);
                        cols.RelativeColumn(1.2f); cols.RelativeColumn(1.6f);
                    });
                    table.Header(header =>
                    {
                        foreach (var titulo in new[] { "Fecha", "Empleado", "Área", "Entrada", "Salida", "Horas", "Retraso", "Estado" })
                            header.Cell().Element(CeldaCabecera).Text(titulo);
                    });
                    foreach (var r in lista)
                    {
                        foreach (var valor in new[]
                        {
                            r.Fecha.ToString("dd/MM/yyyy"), r.NombreCompleto, r.Area,
                            r.HoraEntrada.ToString("HH:mm"), r.HoraSalida?.ToString("HH:mm") ?? "En curso",
                            r.HorasTrabajadas.ToString("0.##"), r.MinutosRetraso.ToString("0"),
                            r.Estado
                        })
                            table.Cell().Element(CeldaCuerpo).Text(valor);
                    }
                });
                page.Footer().Element(PiePagina);
            });
        }).GeneratePdf();
    }

    // ==================== Helpers ====================
    private static void ConfigurarPagina(PageDescriptor page, PageSize size)
    {
        page.Size(size);
        page.Margin(28);
        page.DefaultTextStyle(x => x.FontSize(8.5f).FontColor("#263038"));
    }

    private static void Encabezado(IContainer container, string titulo, DateOnly inicio, DateOnly fin) =>
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(inner =>
                {
                    inner.Item().Text("WORKFORCE MANAGER PRO").FontSize(8).Bold().FontColor("#7d8792");
                    inner.Item().PaddingTop(2).Text(titulo).FontSize(16).Bold().FontColor(Dark);
                    inner.Item().PaddingTop(2).Text($"Período: {inicio:dd/MM/yyyy} — {fin:dd/MM/yyyy}").FontSize(8.5f).FontColor("#7d8792");
                });
                row.ConstantItem(150).AlignRight().Column(inner =>
                {
                    inner.Item().AlignRight().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7.5f).FontColor("#9aa3aa");
                });
            });
            col.Item().PaddingTop(9).LineHorizontal(1).LineColor(Line);
        });

    private static IContainer CeldaCabecera(IContainer c) =>
        c.Background(Dark).PaddingVertical(5).PaddingHorizontal(5)
            .DefaultTextStyle(x => x.FontColor("#ffffff").Bold().FontSize(7.5f));

    private static IContainer CeldaCuerpo(IContainer c) =>
        c.BorderBottom(0.5f).BorderColor(Line).PaddingVertical(4).PaddingHorizontal(5);

    private static void PiePagina(IContainer container) =>
        container.PaddingTop(6).BorderTop(0.5f).BorderColor(Line).Row(row =>
        {
            row.RelativeItem().Text("SIGTA · Reporte generado automáticamente").FontSize(7).FontColor("#9aa3aa");
            row.ConstantItem(90).AlignRight().Text(t =>
            {
                t.DefaultTextStyle(x => x.FontSize(7).FontColor("#9aa3aa"));
                t.Span("Página ");
                t.CurrentPageNumber();
                t.Span(" de ");
                t.TotalPages();
            });
        });

    private static void Titulo(IXLWorksheet ws, string titulo, DateOnly inicio, DateOnly fin, int columnas)
    {
        ws.Cell(1, 1).Value = "WORKFORCE MANAGER PRO";
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(Dark);
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 9;
        ws.Cell(2, 1).Value = $"{titulo} · {inicio:dd/MM/yyyy} — {fin:dd/MM/yyyy}";
        ws.Cell(2, 1).Style.Font.Bold = true;
        ws.Cell(2, 1).Style.Font.FontSize = 13;
        ws.Range(1, 1, 2, columnas).Merge();
    }

    private static void EstiloCabecera(IXLRange range)
    {
        range.Style.Fill.BackgroundColor = XLColor.FromHtml(Dark);
        range.Style.Font.FontColor = XLColor.White;
        range.Style.Font.Bold = true;
        range.Style.Font.FontSize = 9;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    }

    private static byte[] Guardar(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
