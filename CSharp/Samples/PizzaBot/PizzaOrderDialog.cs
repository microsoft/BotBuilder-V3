using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Form;
using Microsoft.Bot.Builder.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Sample.PizzaBot
{
#pragma warning disable CS1998

    [LuisModel("https://api.projectoxford.ai/luis/v1/application?id=a19f7eee-0280-4a9a-b5e5-73c16b32c43d&subscription-key=fe054e042fd14754a83f0a205f6552a5&q=")]
    [Serializable]
    public class PizzaOrderDialog : LuisDialog
    {
        private readonly Func<IForm<PizzaOrder>> MakePizzaForm;

        internal PizzaOrderDialog(Func<IForm<PizzaOrder>> makePizzaForm)
        {
            this.MakePizzaForm = makePizzaForm;
        }

        protected PizzaOrderDialog(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Field.SetNotNullFrom(out this.MakePizzaForm, nameof(MakePizzaForm), info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.MakePizzaForm), MakePizzaForm);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry. I didn't understand you.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("OrderPizza")]
        [LuisIntent("UseCoupon")]
        public async Task ProcessPizzaForm(IDialogContext context, LuisResult result)
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

            // TODO: pass initial state
            var pizzaForm = this.MakePizzaForm();
            context.Call<IForm<PizzaOrder>, PizzaOrder>(pizzaForm, PizzaFormComplete);
        }

        private async Task PizzaFormComplete(IDialogContext context, IAwaitable<PizzaOrder> result)
        {
            var order = await result;
            if (order != null)
            {
                await context.PostAsync("Your Pizza Order: " + result.ToString());
            }
            else
            {
                await context.PostAsync("Form returned empty response!");
            }
        }

        enum Days { Saturday, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday }; 

        [LuisIntent("StoreHours")]
        public async Task ProcessStoreHours(IDialogContext context, LuisResult result)
        {
            var days = (IEnumerable<Days>)Enum.GetValues(typeof(Days));

            Prompts.Choice(context, StoreHoursResult, days, "Which day of the week?");
        }

        private async Task StoreHoursResult(IDialogContext context, IAwaitable<Days> day)
                {
                    var hours = string.Empty;
            switch (await day)
                    {
                        case Days.Saturday:
                        case Days.Sunday:
                            hours = "5pm to 11pm";
                            break;
                        default:
                            hours = "11am to 10pm";
                            break;
                    }

            var text = $"Store hours are {hours}";
            await context.PostAsync(text);

            context.Wait(MessageReceived);
        }
    }
}