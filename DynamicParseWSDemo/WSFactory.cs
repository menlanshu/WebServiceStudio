using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Web.Services.Description;
using System.IO;
using System.Linq;

namespace DynamicParseWSDemo
{
    public class WSFactory
    {
        public static GeneratedWebServiceInfo CreateWebService(string webServiceAsmxUrl)
        {
            System.Net.WebClient client = new System.Net.WebClient();

            // Connect To the web service
            Stream stream = client.OpenRead(webServiceAsmxUrl + "?wsdl");

            return Run(stream);
        }

        private static GeneratedWebServiceInfo Run(Stream wsdlStream)
        {
            ServiceDescriptionImporter importer = GetServiceDescriptionImporter(wsdlStream);

            // Initialize a Code-DOM tree into which we will import the service.
            CodeNamespace nmspace = new CodeNamespace();
            CodeCompileUnit unit = new CodeCompileUnit();
            unit.Namespaces.Add(nmspace);

            // Import the service into the Code-DOM tree. This creates proxy code
            // that uses the service.
            ServiceDescriptionImportWarnings warning = importer.Import(nmspace, unit);

            if (warning == 0)
            {
                CompilerResults results = GetCompilerResults(unit);

                // Check For Errors
                if (results.Errors.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("========Compiler error============");
                    foreach (CompilerError compilerError in results.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(compilerError.ErrorText);
                    }
                    throw new Exception("Compile Error Occured calling webservice. Check Debug ouput window.");
                }

                var getFistType = results.CompiledAssembly.GetTypes();
                GeneratedWebServiceInfo generatedWebServiceInfo = new GeneratedWebServiceInfo(getFistType[0]);

                return generatedWebServiceInfo;
                // object wsvcClass = results.CompiledAssembly.CreateInstance(serviceName);
            }
            else
            {
                // Print an error message.
                throw new Exception(warning.ToString());
            }
        }

        private static CompilerResults GetCompilerResults(CodeCompileUnit unit)
        {
            // Generate and print the proxy code in C#.
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            //using (TextWriter writeGenerateCodeToFile = File.CreateText("temp1.cs"))
            //{
            //    provider.GenerateCodeFromCompileUnit(unit, null, new CodeGeneratorOptions());

            //}                

            // Compile the assembly proxy with the appropriate references
            string[] assemblyReferences = new string[5] { "System.dll", "System.Web.Services.dll", "System.Web.dll", "System.Xml.dll", "System.Data.dll" };

            CompilerParameters parms = new CompilerParameters(assemblyReferences);

            CompilerResults results = provider.CompileAssemblyFromDom(parms, unit);

            return results;
        }

        private static ServiceDescriptionImporter GetServiceDescriptionImporter(Stream wsdlStream)
        {
            // Get a WSDL file describing a service.
            ServiceDescription description = ServiceDescription.Read(wsdlStream);

            // Initialize a service description importer.
            ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
            importer.ProtocolName = "Soap12";  // Use SOAP 1.2.
            importer.AddServiceDescription(description, null, null);

            // Report on the service descriptions.
            //Console.WriteLine("Importing {0} service descriptions with {1} associated schemas.",
            //                  importer.ServiceDescriptions.Count, importer.Schemas.Count);

            // Generate a proxy client.
            importer.Style = ServiceDescriptionImportStyle.Client;

            // Generate properties to represent primitive values.
            importer.CodeGenerationOptions = System.Xml.Serialization.CodeGenerationOptions.GenerateProperties;

            return importer;
        }


        public static void CallWebService(GeneratedWebServiceInfo generatedWebServiceInfo)
        {
            var instance = generatedWebServiceInfo.CreateInstance();
            var methodNames = generatedWebServiceInfo.MethodNames.ToList();

            var parameters = generatedWebServiceInfo.GetInParametersOfMethod(methodNames[0]);
            var outParameter = generatedWebServiceInfo.GetOutParameterOfMethod(methodNames[0]);

            List<object> inParaValue = new List<object>();
            foreach (var parameter in parameters)
            {
                var paraInstance = Activator.CreateInstance(parameter.ParameterType);
                foreach (var property in parameter.ParameterType.GetProperties())
                {
                    property.SetValue(paraInstance, "Test");
                    //if (parameter.ParameterType == typeof(string))
                    //{
                    //    string strValue = "Test";
                    //    inParaValue.Add(strValue);
                    //}
                }
                inParaValue.Add(paraInstance);
            }

            var outputResult = generatedWebServiceInfo.InvokeMethod(instance, methodNames[0], inParaValue.ToArray());
        }

    }
}
