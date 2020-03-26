import { Http } from "ur-http";
import { Operation } from "ur-jsonpatch";

export interface Spooler {
    id: number;
    projectId: number;
    zoneId: number;
    name: string;
    key: string;
    printers: string[];
}

export const create = (command: { zoneId: number, name: string }) => Http.post(`/Zones/${command.zoneId}/Spoolers`).withJson(command).expectJson<Spooler>();
export const destroy = (id: number) => Http.delete(`/Spoolers/${id}`);
export const setPrinters = (spoolerId: number, command: { printerNames: string[] }) => Http.put(`/Spoolers/${spoolerId}/Printers`).withJson(command);
export const update = (id: number, patch: Operation[]) => Http.patch(`/Spoolers/${id}`).withJsonPatch(patch).expectJson<Spooler>();

export const getAll = () => Http.get("/Spoolers").expectJson<Spooler[]>();
export const getAllByZoneId = (zoneId: number) => Http.get(`/Zones/${zoneId}/Spoolers`).expectJson<Spooler[]>();
export const getById = (id: number) => Http.get(`/Spoolers/${id}`).expectJson<Spooler>();
export const getByKey = (key: string) => Http.get(`/Spoolers/${key}`).expectJson<Spooler>();