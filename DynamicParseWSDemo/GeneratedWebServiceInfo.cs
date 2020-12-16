using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicParseWSDemo
{
    public class GeneratedWebServiceInfo
    {
        private readonly string _requestNameSpaceName = "RequestNamespace";

        public Type WebServiceType { get; private set; }
        public ICollection<CustomMethodInfo> CustomMethods { get; private set; }
        public ICollection<string> MethodNames {
            get 
            {
                return CustomMethods.Select(x => x.Method.Name).ToList();
            } 
        }

        public GeneratedWebServiceInfo(Type webServiceType)
        {
            WebServiceType = webServiceType;

            GenerateCustomMethods();
        }

        public object CreateInstance()
        {
            return Activator.CreateInstance(WebServiceType);
        }

        public ICollection<ParameterInfo> GetInParametersOfMethod(string methodName)
        {
            return CustomMethods.FirstOrDefault(x => x.Method.Name == methodName)?.InParameterInfo;
        }

        public ParameterInfo GetOutParameterOfMethod(string methodName)
        {
            return CustomMethods.FirstOrDefault(x => x.Method.Name == methodName)?.OutParameterInfo;
        }

        public object InvokeMethod(object currentInstance, string methodName, object[] args)
        {
            MethodInfo mi = CustomMethods.FirstOrDefault(x => x.Method.Name == methodName).Method;

            return mi?.Invoke(currentInstance, args);
        }


        private void GenerateCustomMethods()
        {
            CustomMethods = new List<CustomMethodInfo>();

            IEnumerable<MethodInfo> methods = WebServiceType.GetMethods().Where(x => 
                x.CustomAttributes.ToList().Any(customAttribute =>
                customAttribute.NamedArguments.Any(argument => argument.MemberName == _requestNameSpaceName)));

            AppendAllMethodsToCustomMethodInfo(methods);
        }

        private void AppendAllMethodsToCustomMethodInfo(IEnumerable<MethodInfo> methods)
        {
            foreach (var method in methods)
            {
                CustomMethodInfo customMethodInfo = new CustomMethodInfo
                {
                    Method = method,
                    InParameterInfo = method.GetParameters(),
                    OutParameterInfo = method.ReturnParameter
                };

                CustomMethods.Add(customMethodInfo);
            }
        }
    }


    public class CustomMethodInfo
    {
        public MethodInfo Method { get; set; }
        public ICollection<ParameterInfo> InParameterInfo { get; set; }
        public ParameterInfo OutParameterInfo { get; set; }
    }
}
