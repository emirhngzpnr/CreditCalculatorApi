using CreditCalculatorApi.Entities;
using CreditCalculatorApi.DTOs;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace CreditCalculatorApi.Services
{
    public class PdfService
    {
        private readonly IConverter _converter;

        public PdfService(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] GeneratePdf(string htmlContent)
        {
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = htmlContent
                    }
                }
            };

            return _converter.Convert(doc);
        }

        public string GenerateInstallmentHtml(CreditApplication app, List<InstallmentDto> installments)
        {
            return $@"
    <html>
    <head>
        <meta charset='UTF-8'>
        <style>
            body {{ font-family: Arial; font-size: 14px; }}
            table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
            th, td {{ border: 1px solid #ccc; padding: 8px; text-align: right; }}
            th {{ background-color: #f2f2f2; }}
            h2, h3 {{ text-align: left; }}
            p {{ margin: 4px 0; }}
        </style>
    </head>
    <body>
        <h2>Kredi Başvuru Özeti</h2>
        <p><strong>Ad Soyad:</strong> {app.FullName}</p>
        <p><strong>Banka:</strong> {app.BankName}</p>
        <p><strong>Kredi Türü:</strong> {app.CreditType}</p>
        <p><strong>Kredi Tutarı:</strong> {app.CreditAmount} TL</p>
        <p><strong>Vade:</strong> {app.CreditTerm} ay</p>

        <h3>Ödeme Planı</h3>
        <table>
            <tr>
                <th>Taksit No</th>
                <th>Taksit Tutarı</th>
                <th>Anapara</th>
                <th>Faiz</th>
                <th>Kalan Anapara</th>
            </tr>
            {GenerateRows(installments)}
        </table>
    </body>
    </html>";
        }

        private string GenerateRows(List<InstallmentDto> installments)
        {
            string rows = "";

            foreach (var item in installments)
            {
                rows += $"<tr>" +
                        $"<td>{item.InstallmentNo}</td>" +
                        $"<td>{item.Payment:N2} TL</td>" +
                        $"<td>{item.Principal:N2} TL</td>" +
                        $"<td>{item.Interest:N2} TL</td>" +
                        $"<td>{item.RemainingPrincipal:N2} TL</td>" +
                        $"</tr>";
            }

            return rows;
        }
    }
}
