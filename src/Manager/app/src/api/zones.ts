import { Http } from "ur-http";
import { Operation } from "ur-jsonpatch";

export interface Zone {
    id: number;
    projectId: number;
    name: string;
    routes: { [alias: string]: ZoneRoute };
}

export interface ZoneRoute {
    zoneId: number;
    alias: string;
    spoolerId: number;
    printerName: string;
}

export const create = (command: { name: string, routes?: { alias: string, spoolerId: number, printerName: string }[] }) => Http.post("/Zones").withJson(command).expectJson<Zone>();
export const destroy = (id: number) => Http.delete(`/Zones/${id}`);
export const update = (id: number, patch: Operation[]) => Http.patch(`/Zones/${id}`).withJsonPatch(patch).expectJson<Zone>();

export const getAll = () => Http.get("/Zones").expectJson<Zone[]>();
export const getById = (id: number) => Http.get(`/Zones/${id}`).expectJson<Zone>();