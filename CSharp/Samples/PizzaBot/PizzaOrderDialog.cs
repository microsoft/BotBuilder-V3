using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Form;
using Microsoft.Bot.Builder.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Sample.PizzaBot
{
    [LuisModel("https://api.projectoxford.ai/luis/v1/application?id=a19f7eee-0280-4a9a-b5e5-73c16b32c43d&subscription-key=fe054e042fd14754a83f0a205f6552a5&q=")]
    public class PizzaOrderDialog : LuisDialog<string, DialogResult>
    {
        private readonly IForm<PizzaOrder> pizzaForm;

        internal PizzaOrderDialog(IForm<PizzaOrder> pizzaForm)
        {
            this.pizzaForm = pizzaForm;
        }

        [LuisIntent("")]
        public async Task<DialogResponse> None(ISession session, LuisResult result)
        {
            return await session.CreateDialogResponse("I'm sorry. I didn't understand you.");
        }

        [LuisIntent("OrderPizza")]
        [LuisIntent("UseCoupon")]
        public async Task<DialogResponse> ProcessPizzaForm(ISession session, LuisResult result)
        {
            var initialState = new Form<PizzaOrder>.InitialState();
            var entities = new List<EntityRecommendation>(result.Entities);
            if (!entities.Any((entity) => entity.Type == "Kind"))
            {
                // Infer kind
                foreach(var entity in result.Entities)
                {
                    string kind = null;
                    switch(entity.Type)
                    {
                        case "Signature": kind = "Signature"; break;
                        case "GourmetDelite": kind = "Gourmet delite"; break;
                        case "Stuffed": kind = "stuffed"; break;
                        default: if (entity.Type.StartsWith("BYO")) kind = "byo";
                            break;
                    }
                    if (kind != null)
                    {
                        entities.Add(new EntityRecommendation("Kind") { Entity = kind });
                        break;
                    }
                }
            }
            initialState.Entities = entities.ToArray();
            initialState.State = null;
            return await session.BeginDialogAsync(this.pizzaForm, initialState);
        }

        [LuisIntent("OrderPizza", resumeHandler: true)]
        [LuisIntent("UseCoupon", resumeHandler:true)]
        public async Task<DialogResponse> PizzaFormComplete(ISession session, DialogResult dialogResult)
        {
            var result = dialogResult as FormResult<PizzaOrder>;
            if (result?.State != null)
            {
                return await session.CreateDialogResponse("Your Pizza Order: " + JsonConvert.SerializeObject(result.State));
            }
            else
            {
                return await session.CreateDialogResponse("Form returned empty response!");
            }
        }

        enum Days { Saturday, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday }; 

        [LuisIntent("StoreHours")]
        public async Task<DialogResponse> ProcessStoreHours(ISession session, LuisResult result)
        {
            return await Prompts.Choice(session, "Which day of the week?", Enum.GetValues(typeof(Days)).Cast<Days>().Select(s => s.ToString()).ToList());
        }

        [LuisIntent("StoreHours", resumeHandler: true)]
        public async Task<DialogResponse> StoreHoursResult(ISession session, PromptDialogResult<string> dialogResult)
        {
            if(dialogResult != null && dialogResult.Completed && !string.IsNullOrEmpty(dialogResult.Response))
            {
                var hours = string.Empty;
                switch((Days)Enum.Parse(typeof(Days), dialogResult.Response))
                {
                    case Days.Saturday:
                    case Days.Sunday:
                        hours = "5pm to 11pm";
                        break;
                    default:
                        hours = "11am to 10pm";
                        break;
                }

                return await session.CreateDialogResponse(string.Format("Store hours for {0} is {1}", dialogResult.Response, hours));
            }
            else
            {
                return await session.CreateDialogResponse("I didn't get which day of the week you are referring to!");
            }
        }
    }
}