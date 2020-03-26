import { Http } from "ur-http";

export interface Project {
    id: number;
    key: string;
    name: string;
}

export const getByKey = (key: string) => Http.get(`/Projects/${key}`).expectJson<Project>();