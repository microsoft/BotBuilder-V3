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

import utils = require('../utils');

export interface IBotStorageAddress {
    userId?: string;
    conversationId?: string;
}

export interface IBotStorageData {
    userData?: any;
    conversationData?: any;
}

export interface IBotStorage {
    get(address: IBotStorageAddress, callback: (err: Error, data: IBotStorageData) => void): void;
    save(address: IBotStorageAddress, data: IBotStorageData, callback?: (err: Error) => void): void;
}

export class MemoryBotStorage implements IBotStorage {
    private userStore: { [id: string]: string; } = {};
    private conversationStore: { [id: string]: string; } = {};

    public get(address: IBotStorageAddress, callback: (err: Error, data: IBotStorageData) => void): void {
        var data: IBotStorageData = {};
        if (address.userId) {
            if (this.userStore.hasOwnProperty(address.userId)) {
                data.userData = JSON.parse(this.userStore[address.userId]);
            } else {
                data.userData = null;
            }
        }
        if (address.conversationId) {
            if (this.conversationStore.hasOwnProperty(address.conversationId)) {
                data.conversationData = JSON.parse(this.conversationStore[address.conversationId]);
            } else {
                data.conversationData = null;
            }
        }
        callback(null, data);
    }

    public save(address: IBotStorageAddress, data: IBotStorageData, callback?: (err: Error) => void): void {
        if (address.userId) {
            this.userStore[address.userId] = JSON.stringify(data.userData || {});
        }
        if (address.conversationId) {
            this.conversationStore[address.conversationId] = JSON.stringify(data.conversationData || {});
        }
    }

    public delete(address: IBotStorageAddress) {
        if (address.userId && this.userStore.hasOwnProperty(address.userId)) {
            delete this.userStore[address.userId];
        }
        if (address.conversationId && this.conversationStore.hasOwnProperty(address.conversationId)) {
            delete this.conversationStore[address.conversationId];
        }
    }
}