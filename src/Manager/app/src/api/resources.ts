import { Http } from "ur-http";

export interface Resource {
    id: number;
    alias: string;
    contentUrl: string;
    contentType: string;
}

export const destroy = (alias: string) => Http.delete(`/Resources/${alias}`);
export const set = (alias: string, content: Uint8Array | string, contentType: string) => Http.put(`/Resources/${alias}/Content`).with(content, contentType).expectJson<Resource>();

export const getAll = () => Http.get("/Resources").expectJson<Resource[]>();
export const getByAlias = (alias: string) => Http.get(`/Resources/${alias}`).expectJson<Resource>();
export const getContentByAlias = (alias: string) => Http.get(`/Resources/${alias}/Content`);