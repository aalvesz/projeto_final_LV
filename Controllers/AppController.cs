using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace projeto_final_LV.Controllers;

public abstract class AppController : Controller
{
    protected string CidadeAtual =>
        Request.Cookies["CidadeAtual"] ?? string.Empty;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Deixa a cidade disponível para o _Layout em TODAS as telas
        ViewData["CidadeAtual"] = CidadeAtual;
        base.OnActionExecuting(context);
    }
}
