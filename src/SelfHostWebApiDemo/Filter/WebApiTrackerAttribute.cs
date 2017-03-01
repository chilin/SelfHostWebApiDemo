using Common.Logging;
using SelfHostWebApiDemo.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SelfHostWebApiDemo.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class WebApiTrackerAttribute : ActionFilterAttribute
    {
        private static readonly ILog logger = LogManager.GetLogger<WebApiTrackerAttribute>();
        private readonly string Key = "_thisWebApiOnActionMonitorLog_";
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);
            WebApiMonitorLog MonLog = new WebApiMonitorLog();
            MonLog.ExecuteStartTime = DateTime.Now;
            //获取Action 参数
            MonLog.ActionParams = actionContext.ActionArguments;
            MonLog.HttpRequestHeaders = actionContext.Request.Headers.ToString();
            MonLog.HttpMethod = actionContext.Request.Method.Method;

            actionContext.Request.Properties[Key] = MonLog;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            WebApiMonitorLog MonLog = actionExecutedContext.Request.Properties[Key] as WebApiMonitorLog;
            MonLog.ExecuteEndTime = DateTime.Now;
            MonLog.ActionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
            MonLog.ControllerName = actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            logger.Info(MonLog.GetLoginfo());
            if (actionExecutedContext.Exception != null)
            {
                string Msg = string.Format(@"
                请求【{0}Controller】的【{1}】产生异常：
                Action参数：{2}
                Http请求头:{3}
                客户端IP：{4},
                HttpMethod:{5}
                    ", MonLog.ControllerName, MonLog.ActionName, MonLog.GetCollections(MonLog.ActionParams), MonLog.HttpRequestHeaders, MonLog.GetIP(), MonLog.HttpMethod);
                logger.Error(Msg, actionExecutedContext.Exception);
            }
        }
    }
}
