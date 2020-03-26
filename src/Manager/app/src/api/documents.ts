import { Http } from "ur-http";

export interface Document {
    id: number;
    projectId: number;
    terminalId: number;
    templateId?: number;
    contentType: string;
    contentUrl: string;
}

export const create = () => Http.post("/Documents");
export const destroy = (id: number) => Http.delete(`/Documents/${id}`);
export const generate = (zoneId: number) => Http.post(`/Zones/${zoneId}/Documents/Generate`);

export const getAll = () => Http.get("/Documents").expectJson<Document>();
export const geById = (id: number) => Http.get(`/Documents/${id}`).expectJson<Document>();