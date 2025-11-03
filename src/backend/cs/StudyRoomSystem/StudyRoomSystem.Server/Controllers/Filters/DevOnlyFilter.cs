using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StudyRoomSystem.Server.Controllers.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class DevOnlyAttribute : Attribute;

/// <summary>
/// 开发模式下可用的接口
/// </summary>
// public class DevOnlyFilter : IActionFilter
// {
//     private IWebHostEnvironment Environment { get; }
//     
//     public DevOnlyFilter(IWebHostEnvironment environment)
//     {
//         Environment = environment;
//     }
//     
//     public void OnActionExecuting(ActionExecutingContext context)
//     {
//         var devOnly = context.ActionDescriptor.EndpointMetadata.OfType<DevOnlyAttribute>().Any();
//         
//         if (devOnly && Environment.IsProduction())
//         {
//             context.Result = new NotFoundResult();
//         }
//     }
//     public void OnActionExecuted(ActionExecutedContext context)
//     {
//         
//     }
// }
