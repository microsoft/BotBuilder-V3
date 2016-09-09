// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

import consts = require('./consts');
import utils = require('./utils');
import ses = require('./Session');
import msg = require('./Message');
import dialog = require('./dialogs/Dialog');
import actions = require('./dialogs/DialogAction');
import prompts = require('./dialogs/Prompts');
import intent = require('./dialogs/IntentDialog');
import luis = require('./dialogs/LuisRecognizer');
import simple = require('./dialogs/SimpleDialog');
import entities = require('./dialogs/EntityRecognizer');
import dl = require('./bots/Library');
import ub = require('./bots/UniversalBot');
import chat = require('./bots/ChatConnector');
import cc = require('./bots/ConsoleConnector');
import bs = require('./storage/BotStorage');
import ca = require('./cards/CardAction');
import hero = require('./cards/HeroCard');
import img = require('./cards/CardImage');
import rc = require('./cards/ReceiptCard');
import signin = require('./cards/SigninCard');
import thumb = require('./cards/ThumbnailCard');
import kb = require('./cards/Keyboard');
import middleware = require('./middleware/Middleware');

declare var exports: any;

exports.Session = ses.Session;
exports.Message = msg.Message;
exports.AttachmentLayout = msg.AttachmentLayout;
exports.TextFormat = msg.TextFormat;
exports.CardAction = ca.CardAction;
exports.HeroCard = hero.HeroCard;
exports.CardImage = img.CardImage;
exports.ReceiptCard = rc.ReceiptCard;
exports.ReceiptItem = rc.ReceiptItem;
exports.Fact = rc.Fact;
exports.SigninCard = signin.SigninCard;
exports.ThumbnailCard = thumb.ThumbnailCard;
exports.Keyboard = kb.Keyboard;
exports.Dialog = dialog.Dialog;
exports.ResumeReason = dialog.ResumeReason;
exports.DialogAction = actions.DialogAction;
exports.PromptType = prompts.PromptType;
exports.ListStyle = prompts.ListStyle;
exports.Prompts = prompts.Prompts;
exports.SimplePromptRecognizer = prompts.SimplePromptRecognizer;
exports.IntentDialog = intent.IntentDialog;
exports.RecognizeOrder = intent.RecognizeOrder;
exports.RecognizeMode = intent.RecognizeMode;
exports.LuisRecognizer = luis.LuisRecognizer;
exports.SimpleDialog = simple.SimpleDialog;
exports.EntityRecognizer = entities.EntityRecognizer;
exports.Library = dl.Library;
exports.UniversalBot = ub.UniversalBot;
exports.ChatConnector = chat.ChatConnector;
exports.ConsoleConnector = cc.ConsoleConnector;
exports.MemoryBotStorage = bs.MemoryBotStorage;
exports.Middleware = middleware.Middleware;

// Deprecated classes
import deprecatedBCB = require('./deprecated/BotConnectorBot');
import deprecatedLD = require('./deprecated/LuisDialog');
import deprecatedCD = require('./deprecated/CommandDialog');
import deprecatedTB = require('./deprecated/TextBot');
exports.BotConnectorBot = deprecatedBCB.BotConnectorBot;
exports.LuisDialog = deprecatedLD.LuisDialog;
exports.CommandDialog = deprecatedCD.CommandDialog;
exports.TextBot = deprecatedTB.TextBot;
