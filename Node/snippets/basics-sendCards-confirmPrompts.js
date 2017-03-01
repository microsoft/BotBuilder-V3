// Add dialog to handle 'Buy' button click
bot.dialog('buyButtonClick', [ 
    // ... waterfall steps ... 
]).triggerAction({ 
    matches: /(buy|add)\s.*shirt/i,
    confirmPrompt: "This will cancel adding the current item. Are you sure?" 
}).cancelAction('cancelBuy', "Ok... Item canceled", { 
    matches: /^cancel/i,
    confirmPrompt: "are you sure?" 
});
