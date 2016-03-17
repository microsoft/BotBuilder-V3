import utils = require('../utils');

export interface IStorage {
    get(id: string, callback: (err: Error, data: any) => void): void;
    save(id: string, data: any, callback?: (err: Error) => void): void;
}

export class MemoryStorage implements IStorage {
    private store: { [id: string]: any; } = {};

    public get(id: string, callback: (err: Error, data: any) => void): void {
        if (this.store.hasOwnProperty(id)) {
            callback(null, utils.clone(this.store[id]));
        } else {
            callback(null, null);
        }
    }

    public save(id: string, data: any, callback?: (err: Error) => void): void {
        this.store[id] = utils.clone(data || {});
        if (callback) {
            callback(null);
        }
    }

    public delete(id: string) {
        if (this.store.hasOwnProperty(id)) {
            delete this.store[id];
        }
    }
}