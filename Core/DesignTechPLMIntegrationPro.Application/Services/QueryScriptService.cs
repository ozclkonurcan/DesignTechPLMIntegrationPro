using DesignTechPLMIntegrationPro.Domain.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Application.Services
{
	public class QueryScriptService
	{
		private readonly string _jsonFilePath;

		public QueryScriptService()
		{
			_jsonFilePath = @"C:\Users\tronu\source\repos\designtechkamil\DesignTechPLMIntegrationPro\Core\DesignTechPLMIntegrationPro.Application\Scripts\SqlQueryList.json";
		}

		public List<SqlScriptDefinition> GetScripts()
		{
			var json = File.ReadAllText(_jsonFilePath);
			return JsonConvert.DeserializeObject<List<SqlScriptDefinition>>(json);
		}
	}
}
