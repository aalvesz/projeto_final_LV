using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using projeto_final_LV.Models;

public class CatalogController : Controller
{
    public IActionResult Catalog()
    {
        return View();
    }

/*
[HttpPost]
public IActionResult ExportarCatalogo()
{
    var cidade = TempData["CidadeAtual"]?.ToString() ?? "São Paulo";
    TempData.Keep("CidadeAtual");

    var clima = _climaService.ObterClima(cidade);

    // Pega os mesmos filmes que aparecem na tela
    var generosRecomendados = _climaService.ObterGenerosRecomendados(clima.Condicao);

    var filmes = _context.Filmes
        .Where(f => generosRecomendados.Contains(f.Genero))
        .Select(f => new
        {
            Título = f.Titulo,
            Ano = f.Ano,
            Gênero = f.Genero,
            Nota = f.Nota,
            Sinopse = f.Sinopse?.Length > 100 ? f.Sinopse.Substring(0, 97) + "..." : f.Sinopse
        })
        .ToList();

    // Cria o Excel com ClosedXML
    using var workbook = new ClosedXML.Excel.XLWorkbook();
    var worksheet = workbook.Worksheets.Add("Catálogo Atmos");

    // Título bonito
    worksheet.Cell(1, 1).Value = $"Catálogo de filmes recomendado para {cidade}";
    worksheet.Cell(2, 1).Value = $"Clima atual: {clima.Condicao} • {clima.Temperatura}°C • {DateTime.Now:dd/MM/yyyy HH:mm}";
    worksheet.Range("A1:E1").Merge().Style.Font.Bold = true; 
    worksheet.Range("A1:E1").Style.Font.FontSize = 16;
    worksheet.Range("A2:E2").Merge().Style.Font.Italic = true;

    // Cabeçalho da tabela
    worksheet.Cell(4, 1).Value = "Título";
    worksheet.Cell(4, 2).Value = "Ano";
    worksheet.Cell(4, 3).Value = "Gênero";
    worksheet.Cell(4, 4).Value = "Nota";
    worksheet.Cell(4, 5).Value = "Sinopse";
    worksheet.Range("A4:E4").Style.Font.Bold = true;
    worksheet.Range("A4:E4").Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(0, 102, 204);
    worksheet.Range("A4:E4").Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

    // Dados
    var row = 5;
    foreach (var f in filmes)
    {
        worksheet.Cell(row, 1).Value = f.Título;
        worksheet.Cell(row, 2).Value = f.Ano;
        worksheet.Cell(row, 3).Value = f.Gênero;
        worksheet.Cell(row, 4).Value = f.Nota;
        worksheet.Cell(row, 5).Value = f.Sinopse;
        row++;
    }

    // Auto ajuste de colunas
    worksheet.Columns().AdjustToContents();

    // Gera o arquivo
    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    var content = stream.ToArray();

    // Nome do arquivo com cidade e data
    var fileName = $"Atmos_Catalogo_{cidade}_{DateTime.Now:yyyy-MM-dd}.xlsx";

    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
}
*/


}

