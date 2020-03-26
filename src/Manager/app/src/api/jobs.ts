import { Http } from "ur-http";

export const printDocument = (documentId: number, params: { route: string }) => Http.post(`/Documents/${documentId}/Print`, params);
export const printRaw = (spoolerId: number, params: { printerName: string }) => Http.post(`/Spoolers/${spoolerId}/Print`, params);