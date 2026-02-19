export interface McpMetadata {
  name: string;
  description: string;
  version: string;
  company: string;
  owner: string;
  contactEmail?: string;
  status: "active" | "deprecated" | "planned";
  lifecycle:
    | "design"
    | "development"
    | "testing"
    | "preview"
    | "production"
    | "deprecated"
    | "retired";
  authMethod: "none" | "api-key" | "oauth2" | "entra-id";
  endpointUrl?: string;
  documentationUrl?: string;
  tags?: string[];
  customProperties?: Record<string, unknown>;
}

export interface OpenApiSpec {
  openapi: string;
  info: {
    title: string;
    description: string;
    version: string;
    contact: {
      name: string;
      email?: string;
    };
  };
  servers: Array<{
    url: string;
    description: string;
  }>;
  paths: Record<
    string,
    {
      get?: OpenApiOperation;
      post?: OpenApiOperation;
    }
  >;
}

export interface OpenApiOperation {
  summary: string;
  description: string;
  operationId: string;
  responses: Record<
    string,
    {
      description: string;
    }
  >;
}
