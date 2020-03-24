import { Http } from "ur-http";

export interface Resource {
    id: number;
    alias: string;
    contentUrl: string;
    contentType: string;
}

export const destroy = (alias: string) => Http.delete(`/Resources/${alias}`);
export const set = (alias: string, content: File, contentType: string) => Http.put(`/Resources/${alias}`).with(content, contentType);

export const getAll = () => Http.get("/Resources").expectJson<Resource[]>();
export const getByAlias = (alias: string) => Http.get(`/Resources/${alias}`);