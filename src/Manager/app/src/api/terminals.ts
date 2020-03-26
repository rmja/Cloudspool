import { Http } from "ur-http";
import { Operation } from 'ur-jsonpatch';

export interface Terminal {
    id: number;
    projectId: number;
    zoneId: number;
    name: string;
    key: string;
    routes: { [alias: string]: TerminalRoute };
}

export interface TerminalRoute {
    terminalId: number;
    alias: string;
    spoolerId: number;
    printerName: string;
}

export const create = (command: { zoneId: number, name: string, routes?: { alias: string, spoolerId: number, printerName: string }[] }) => Http.post(`/Zones/${command.zoneId}/Terminals`).withJson(command).expectJson<Terminal>();
export const destroy = (id: number) => Http.delete(`/Terminals/${id}`);
export const update = (id: number, patch: Operation[]) => Http.patch(`/Terminals/${id}`).withJsonPatch(patch).expectJson<Terminal>();

export const getAllByZoneId = (zoneId: number) => Http.get(`/Zones/${zoneId}/Terminals`).expectJson<Terminal[]>();
export const getById = (id: number) => Http.get(`//Terminals/${id}`).expectJson<Terminal>();