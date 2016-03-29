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
import session = require('./Session');
import dialog = require('./dialogs/Dialog');
import actions = require('./dialogs/DialogAction');
import collection = require('./dialogs/DialogCollection');
import prompts = require('./dialogs/Prompts');
import intent = require('./dialogs/IntentDialog');
import luis = require('./dialogs/LuisDialog');
import command = require('./dialogs/CommandDialog');
import simple = require('./dialogs/SimpleDialog');
import entities = require('./dialogs/EntityRecognizer');
import storage = require('./storage/Storage');
import connector = require('./bots/BotConnectorBot');
import skype = require('./bots/SkypeBot');
import slack = require('./bots/SlackBot');
import text = require('./bots/TextBot');

declare var exports: any;

exports.Session = session.Session;
exports.Dialog = dialog.Dialog;
exports.ResumeReason = dialog.ResumeReason;
exports.DialogAction = actions.DialogAction;
exports.DialogCollection = collection.DialogCollection;
exports.PromptType = prompts.PromptType;
exports.ListStyle = prompts.ListStyle;
exports.Prompts = prompts.Prompts;
exports.SimplePromptRecognizer = prompts.SimplePromptRecognizer;
exports.IntentDialog = intent.IntentDialog;
exports.IntentGroup = intent.IntentGroup;
exports.LuisDialog = luis.LuisDialog;
exports.CommandDialog = command.CommandDialog;
exports.EntityRecognizer = entities.EntityRecognizer;
exports.MemoryStorage = storage.MemoryStorage;
exports.BotConnectorBot = connector.BotConnectorBot;
exports.BotConnectorSession = connector.BotConnectorSession;
exports.SkypeBot = skype.SkypeBot;
exports.SkypeSession = skype.SkypeSession;
exports.SlackBot = slack.SlackBot;
exports.SlackSession = slack.SlackSession;
exports.TextBot = text.TextBot;

