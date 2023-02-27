public class Script : ScriptBase
{
    public override async Task<HttpResponseMessage> ExecuteAsync()
    {
        return await this.HandleForwardOperation().ConfigureAwait(false);
    }
    private async Task<HttpResponseMessage> HandleForwardOperation()
    {
		if(this.Context.OperationId == "Portal_list" || this.Context.OperationId == "Delete_Trigger"){
			return await this.HandlePortalList().ConfigureAwait(false);
		}
        var dict = new Dictionary<string, string>();
		JObject jObj = JObject.Parse(await this.Context.Request.Content.ReadAsStringAsync()); 
		
		if (this.Context.OperationId == "Request_trigger" || this.Context.OperationId == "Change_trigger"){		
            var triggerModuleName = "event_subscription";
			foreach (JProperty property in jObj.Properties())
			{
                var inputData = JObject.Parse(jObj[property.Name].ToString());
				var triggerObj = (JObject)inputData[triggerModuleName];
                var executeOn = (String)triggerObj["execute_on"];
				var triggerName = (String)triggerObj["name"];
                triggerName = triggerName + "_" + DateTime.Now.ToFileTime().ToString();
                triggerObj["name"] = triggerName;
				var action = (JObject)triggerObj["actions"];
				var actionObj = (JObject)action["action"];
				var webhook = (JObject)actionObj["webhook"];
                var urlString = (String)webhook["url"];
                String[] url = urlString.Split('?');
                webhook["url"] = url[0];
                char[] separator = {'=', '&'};
                String[] param = url[1].Split(separator);
                JObject paramValue = new JObject();
                for (int i = 0; i < param.Length; i++){
                    paramValue[param[i]] = HttpUtility.UrlDecode(param[++i]);
                }
				webhook["params"] = paramValue;
				JArray arr = new JArray();
                arr.Add(action);
				triggerObj["actions"] = arr;

                switch (executeOn) {
                    case "Create":
                        triggerObj["execute_on"] = 1;
                        break;
                    case "Edit":
                        triggerObj["execute_on"] = 2;
                        break;
					case "Create Edit":
                        triggerObj["execute_on"] = 3;
                        break;
                    case "Delete":
                        triggerObj["execute_on"] = 4;
                        break;
					case "Create Delete":
                        triggerObj["execute_on"] = 5;
                        break;
					case "Edit Delete":
                        triggerObj["execute_on"] = 6;
                        break;
					case "Create Edit Delete":
                        triggerObj["execute_on"] = 7;
                        break;
                }
            	dict.Add(property.Name, inputData.ToString());
			}
		 }
		 else{
			 foreach (JProperty property in jObj.Properties())
			{
				dict.Add(property.Name, jObj[property.Name].ToString());
			}
		}
		this.Context.Request.Content = new FormUrlEncodedContent(dict);
		HttpResponseMessage response = await this.Context.SendAsync(this.Context.Request, this.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (response.IsSuccessStatusCode)
		{
			var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
		    response.Content = CreateJsonContent(responseString);
        }
        return response;
    }

    private async Task<HttpResponseMessage> HandlePortalList()
    {
        HttpResponseMessage response = await this.Context.SendAsync(this.Context.Request, this.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        return response;
    }
}
