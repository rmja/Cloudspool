import { IRootScopeService } from 'angular';

export class ResourceCache<T, TKey extends string | number> {
    private items: T[] = [];
    private lastModified = new Date();

    constructor(private $rootScope: IRootScopeService, private getKey: (item: T) => TKey) {
		this.getLastModified = this.getLastModified.bind(this);
    }

    getLastModified() {
        return this.lastModified;
    }

    setAll(items: T[]) {
        this.items.splice(0, this.items.length, ...items);
        this.setLastModified();
        return this.items;
    }

    add(item: T) {
        this.items.push(item);
        this.setLastModified();
        return item;
    }

    ensure(item: T) {
        const index = this.getIndex(this.getKey(item));

        if (index === -1) {
            this.items.push(item);
        }
        else {
            this.items[index] = item;
        }
        this.setLastModified();
        return item;
    }

    ensureAll(items: T[]) {
        return items.map(item => this.ensure(item));
    }

    remove(key: TKey) {
        const index = this.getIndex(key);

        if (index >= 0) {
            this.items.splice(index, 1);
            this.setLastModified();
        }
    }

    get(keyOrPredicate: TKey | ((item: T) => boolean)) {
        if (isKey(keyOrPredicate)) {
            const index = this.getIndex(keyOrPredicate);
            return this.items[index];
        }
        else {
            for (let i = 0; i < this.items.length; i++) {
                if (keyOrPredicate(this.items[i])) {
                    return this.items[i];
                }
            }
        }

        function isKey(x: any): x is TKey {
            return typeof x === "number" || typeof x === "string";
        }
    }

    getAll(predicate?: (item: T) => boolean) {
        return predicate ? this.items.filter(predicate) : [...this.items];
    }

    private getIndex(key: TKey) {
        for (let i = 0; i < this.items.length; i++) {
            if (this.getKey(this.items[i]) === key) {
                return i;
            }
        }
        return -1;
    }

    private setLastModified() {
        this.$rootScope.$apply(() => this.lastModified = new Date());
    }
}