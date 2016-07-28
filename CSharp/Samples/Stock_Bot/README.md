# Stock Bot Samples

The Stock Bot samples show how to build a bot in stages, initially calling out to an existing Web Service (Yahoo Stock Service), then adding a language understanding model [LUIS](http://lus.ai), and then using the Bot Framework SDK LUIS Dialog model.

There are several samples in this directory.
* [Basic_Stock_Bot](Basic_Stock_Bot/) -- This sample shows how to build a basic bot that uses a Web Service (Yahoo Stock service) to take user input, call the Web Service and return the results to the user - there isn't any language understanding model in this sample
* [Basic_Luis_StockBot](Basic_Luis_StockBot/) -- This sample grows on the previous sample and shows how to call a LUIS model, parse results from the LUIS model, and use the State Store to store/retrieve information.
* [LuisDialog_Stock_Bot](LuisDialog_Stock_Bot/) -- this sample is functionally the same as the Basic_Luis_StockBot but uses the LUIS Dialog model.

Also included in this folder is the LUIS model JSON file.
* [Luis_Model](Luis_Model/) -- this is the LUIS model for the Stock Bot samples.
