// @/utils/InventoryMapper.ts

export interface RawStockRecord {
  idStock: number;
  idSku: number;
  idCliente: number;
  idUbicacion: number;
  cantidadActual: number;
  fechaActualizacion: string;
  codigoSku: string;
  nombreSku: string;
  codigoUbicacion: string;
  descripcion?: string;
  unidadMedida?: string;
}

export interface GroupedSku {
  idSku: string;
  codigoSku: string;
  nombreSku: string;
  tipoItem: string;
  totalCantidad: number;
  lotes: {
    idLote: string;
    idStock: number;
    ubicacion: string;
    cantidad: number;
    fechaLimite: string;
    estado: string;
  }[];
}

export function groupStockBySku(items: RawStockRecord[]): GroupedSku[] {
  const map = new Map<string, GroupedSku>();

  for (const item of items) {
    const groupKey = String(item.codigoSku || item.idSku);

    if (!map.has(groupKey)) {
      map.set(groupKey, {
        idSku: groupKey,
        codigoSku: item.codigoSku || String(item.idSku),
        nombreSku: item.nombreSku || "Sin Nombre",
        tipoItem: item.unidadMedida || "Unidades",
        totalCantidad: 0,
        lotes: [],
      });
    }

    const group = map.get(groupKey)!;
    const qty = Number(item.cantidadActual) || 0;
    group.totalCantidad += qty;

    group.lotes.push({
      idLote: `STK-${item.idStock}`,
      idStock: item.idStock,
      ubicacion: item.codigoUbicacion || "Sin Ubicación",
      cantidad: qty,
      fechaLimite: item.fechaActualizacion ? new Date(item.fechaActualizacion).toLocaleDateString("es-CR") : "N/A",
      estado: qty > 0 ? "Disponible" : "Agotado",
    });
  }

  return Array.from(map.values());
}