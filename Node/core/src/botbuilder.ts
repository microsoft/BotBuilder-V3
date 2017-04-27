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

export { Session } from './Session';
export { Message, AttachmentLayout, TextFormat, InputHint } from './Message';
export { CardAction } from './cards/CardAction';
export { HeroCard } from './cards/HeroCard';
export { VideoCard } from './cards/VideoCard';
export { AnimationCard } from './cards/AnimationCard';
export { MediaCard } from './cards/MediaCard';
export { CardImage } from './cards/CardImage';
export { ReceiptCard, ReceiptItem, Fact } from './cards/ReceiptCard';
export { SigninCard } from './cards/SigninCard';
export { ThumbnailCard } from './cards/ThumbnailCard';
export { Keyboard } from './cards/Keyboard';
export { Dialog, ResumeReason } from './dialogs/Dialog';
export { DialogAction } from './dialogs/DialogAction';
export { Prompt, PromptType, ListStyle } from './dialogs/Prompt';
export { Prompts } from './dialogs/Prompts';
export { PromptAttachment } from './dialogs/PromptAttachment';
export { PromptChoice } from './dialogs/PromptChoice';
export { PromptConfirm } from './dialogs/PromptConfirm';
export { PromptNumber } from './dialogs/PromptNumber';
export { PromptRecognizers } from './dialogs/PromptRecognizers';
export { PromptText } from './dialogs/PromptText';
export { PromptTime } from './dialogs/PromptTime';
export { IntentRecognizer } from './dialogs/IntentRecognizer';
export { IntentRecognizerSet, RecognizeOrder } from './dialogs/IntentRecognizerSet';
export { IntentDialog, RecognizeMode } from './dialogs/IntentDialog';
export { LuisRecognizer } from './dialogs/LuisRecognizer';
export { RegExpRecognizer } from './dialogs/RegExpRecognizer';
export { LocalizedRegExpRecognizer } from './dialogs/LocalizedRegExpRecognizer';
export { RecognizerFilter } from './dialogs/RecognizerFilter';
export { SimpleDialog } from './dialogs/SimpleDialog';
export { WaterfallDialog } from './dialogs/WaterfallDialog';
export { EntityRecognizer } from './dialogs/EntityRecognizer';
export { Library } from './bots/Library';
export { UniversalBot } from './bots/UniversalBot';
export { ChatConnector } from './bots/ChatConnector';
export { ConsoleConnector } from './bots/ConsoleConnector';
export { MemoryBotStorage } from './storage/BotStorage';
export { Middleware } from './middleware/Middleware';
export { SuggestedActions } from './cards/SuggestedActions';
// Deprecated in version 3.0
export { BotConnectorBot } from './deprecated/BotConnectorBot';
export { LuisDialog } from './deprecated/LuisDialog';
export { CommandDialog } from './deprecated/CommandDialog';
export { TextBot } from './deprecated/TextBot';

// Deprecated in version 3.8
export { SimplePromptRecognizer } from './deprecated/LegacyPrompts';
