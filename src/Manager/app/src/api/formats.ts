import { Http } from "ur-http";

export interface Format {
    zoneId: number;
    alias: string;
    templateId: number;
}

export const destroy = (zoneId: number, alias: string) => Http.delete(`/Zones/${zoneId}/Formats/${alias}`);
export const set = (command: { zoneId: number, alias: string, templateId: number }) => Http.put(`/Zones/${command.zoneId}/Formats/${command.alias}`).withJson({ templateId: command.templateId }).expectJson<Format>();

export const getAllByZone = (zoneId: number) => Http.get(`/Zones/${zoneId}/Formats`).expectJson<Format[]>();
export const getByAlias = (zoneId: number, alias: string) => Http.get(`/Zones/${zoneId}/Formats/${alias}`).expectJson<Format>();