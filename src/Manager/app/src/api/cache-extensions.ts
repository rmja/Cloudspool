import { HttpBuilderOfT, QueryString } from "ur-http";

declare module "ur-http" {
    interface HttpBuilderOfT<T> {
        bypassCache(bypassCache: boolean): HttpBuilderOfT<T>;
    }
}

function bypassCache<T>(this: HttpBuilderOfT<T>, bypassCache: boolean) {
    if (bypassCache) {
        this.addHeader("Cache-Control", "no-cache");
    }
    return this;
}

HttpBuilderOfT.prototype.bypassCache = bypassCache;