using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StudyRoomSystem.Server.Controllers.Filters;

/// <summary>
/// 开发模式下可用的接口
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class DevOnlyAttribute : Attribute;