export enum EstadoLote {
  Vencido = 0,
  Disponible = 1,
  Descartado = 2,
}

export const EstadoLoteLabel: Record<string, string> = {
  "0": "Vencido",
  "1": "Disponible",
  "2": "Descartado",
  "vencido": "Vencido",
  "disponible": "Disponible",
  "descartado": "Descartado",
};

export interface StockItem {
  idSku: string;
  nombreSku: string;
  tipoItem: string;
  idLote: string;
  ubicacion: string;
  cantidad: number;
  fechaLimite: string;
  estado: EstadoLote | string | number;
}

export interface Lote {
  id: string;
  quantity: number;
  location: string;
  limitDate: string;
  status: EstadoLote | string | number;
}

export interface SKUItem {
  id: string;
  name: string;
  itemType: string;
  tenantId: string;
  lotes: Lote[];
}

export interface Tenant {
  id: string;
  name: string;
  code: string;
}

export function formatEstadoLote(status: unknown): string {
  if (status === null || status === undefined) return "N/A";
  const key = String(status).trim().toLowerCase();
  return EstadoLoteLabel[key] ?? String(status);
}