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

import dlg = require('../dialogs/Dialog');
import da = require('../dialogs/DialogAction');
import sd = require('../dialogs/SimpleDialog');
import consts = require('../consts');

export interface IDialogMap {
    [id: string]: dlg.IDialog;
}

export interface ILibraryMap {
    [name: string]: Library;
}

export class Library {
    private dialogs = <IDialogMap>{};
    private libraries = <ILibraryMap>{};

    constructor(public name: string) {
    }

    public dialog(id: string, dialog?: dlg.IDialog | da.IDialogWaterfallStep[] | da.IDialogWaterfallStep): dlg.Dialog {
        var d: dlg.Dialog;
        if (dialog) {
            // Fixup id
            if (id.indexOf(':') >= 0) {
                id = id.split(':')[1];
            }

            // Ensure unique
            if (this.dialogs.hasOwnProperty(id)) {
                throw new Error("Dialog[" + id + "] already exists in library[" + this.name + "].")
            }

            // Wrap dialog and save
            if (Array.isArray(dialog)) {
                d = new sd.SimpleDialog(da.waterfall(dialog));
            } else if (typeof dialog == 'function') {
                d = new sd.SimpleDialog(da.waterfall([<any>dialog]));
            } else {
                d = <any>dialog;
            }
            this.dialogs[id] = d;
        } else if (this.dialogs.hasOwnProperty(id)) {
            d = this.dialogs[id];
        }
        return d;
    }

    public library(lib: Library|string): Library {
        var l: Library;
        if (typeof lib === 'string') {
            if (lib == this.name) {
                l = this;
            } else if (this.libraries.hasOwnProperty(lib)) {
                l = this.libraries[lib];
            } else {
                // Search for lib
                for (var name in this.libraries) {
                    l = this.libraries[name].library(lib);
                    if (l) {
                        break;
                    }
                }
            }
        } else {
            // Save library
            l = this.libraries[lib.name] = <Library>lib;
        }
        return l;
    }

    public findDialog(libName: string, dialogId: string): dlg.Dialog {
        var d: dlg.Dialog;
        var lib = this.library(libName);
        if (lib) {
            d = lib.dialog(dialogId);
        }
        return d;
    }
}

export var systemLib = new Library(consts.Library.system);
