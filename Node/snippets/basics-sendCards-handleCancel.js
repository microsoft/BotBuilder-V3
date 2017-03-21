// Add dialog to handle 'Buy' button click
bot.dialog('buyButtonClick', [ 
    // ... waterfall steps ... 
]).triggerAction({ matches: /(buy|add)\s.*shirt/i })
  .cancelAction('cancelBuy', "Ok... Item canceled", { matches: /^cancel/i });