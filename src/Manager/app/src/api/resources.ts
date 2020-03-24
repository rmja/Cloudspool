import { Http } from "ur-http";
import { Operation } from "ur-jsonpatch";

export interface Resource {
    id: number;
}

// export const create = (command: { name: string, script: string, scriptContentType: string }) => Http.post("/Templates", { name: command.name }).with(command.script, command.scriptContentType).expectJson<Template>();
// export const destroy = (id: number) => Http.delete(`/Templates/${id}`);
// export const setScript = (id: number, script: string, scriptContentType: string) => Http.put(`/Templates/${id}/Script`).with(script, scriptContentType);
// export const update = (id: number, patch: Operation[]) => Http.patch(`/Templates/${id}`).withJsonPatch(patch).expectJson<Template>();

export const getAll = () => Http.get("/Resources").expectJson<Resource[]>();
// export const getById = (id: number) => Http.get(`/Templates/${id}`).expectJson<Template>();
// export const getScriptById = (id: number) => Http.get(`/Templates/${id}/Script`).expectString();