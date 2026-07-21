"use client";

import React, { useState, useEffect, useMemo } from "react";
import { ChevronDown, ChevronRight, Layers, Building2, Loader2, AlertTriangle, Package, Filter } from "lucide-react";
import { Cliente } from "@/types/clientes";
import { formatEstadoLote } from "@/types/inventory";
import { fetchClientes, fetchInventory } from "@/lib/api";

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
  numeroLote?: string;
  NumeroLote?: string;
  numLote?: string;
  lote?: string;
  idLote?: string | number;
  status?: string | number;
  Status?: string | number;
  estado?: string | number;
  fechaVencimiento?: string;
  descripcion?: string;
  unidadMedida?: string;
}

export interface GroupedSku {
  idSku: number;
  codigoSku: string;
  nombreSku: string;
  totalCantidad: number;
  locations: {
    idStock: number;
    numeroLote: string;
    idUbicacion: number;
    codigoUbicacion: string;
    cantidadActual: number;
    status: string;
    fechaVencimiento?: string;
    fechaActualizacion: string;
  }[];
}

export function groupStockBySku(items: RawStockRecord[]): GroupedSku[] {
  const map = new Map<number, GroupedSku>();

  for (const item of items) {
    if (!map.has(item.idSku)) {
      map.set(item.idSku, {
        idSku: item.idSku,
        codigoSku: item.codigoSku || "N/A",
        nombreSku: item.nombreSku || "Sin Nombre",
        totalCantidad: 0,
        locations: [],
      });
    }

    const group = map.get(item.idSku)!;
    const qty = Number(item.cantidadActual) || 0;
    group.totalCantidad += qty;

    const assignedLote =
      item.numeroLote ||
      item.NumeroLote ||
      item.numLote ||
      item.lote ||
      (item.idLote ? String(item.idLote) : "Sin Lote");

    const rawStatus = item.status ?? item.Status ?? item.estado;
    const resolvedStatus = rawStatus !== undefined && rawStatus !== null 
      ? formatEstadoLote(rawStatus)
      : (qty > 0 ? "Disponible" : "Agotado");

    group.locations.push({
      idStock: item.idStock,
      numeroLote: assignedLote,
      idUbicacion: item.idUbicacion,
      codigoUbicacion: item.codigoUbicacion || "N/A",
      cantidadActual: qty,
      status: resolvedStatus,
      fechaVencimiento: item.fechaVencimiento,
      fechaActualizacion: item.fechaActualizacion,
    });
  }

  return Array.from(map.values());
}

export const InventoryTableTree: React.FC<{ token: string }> = ({ token }) => {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [selectedClienteId, setSelectedClienteId] = useState<number | "">("");
  const [rawStock, setRawStock] = useState<RawStockRecord[]>([]);

  const [loadingClientes, setLoadingClientes] = useState<boolean>(true);
  const [loadingStock, setLoadingStock] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const [expandedSkus, setExpandedSkus] = useState<Record<number, boolean>>({});

  const [selectedStatus, setSelectedStatus] = useState<string>("TODOS");
  const [selectedUbicacion, setSelectedUbicacion] = useState<string>("TODAS");

  useEffect(() => {
    async function loadClientes() {
      setLoadingClientes(true);
      try {
        const rawData = await fetchClientes(token);
        if (Array.isArray(rawData)) {
          setClientes(rawData);
          if (rawData.length > 0) {
            const firstId = rawData[0].idCliente ?? rawData[0].id_cliente ?? rawData[0].IdCliente;
            setSelectedClienteId(firstId);
          }
        }
      } catch (err) {
        console.error("[DEBUG] Error fetching clients:", err);
      } finally {
        setLoadingClientes(false);
      }
    }
    loadClientes();
  }, [token]);

  useEffect(() => {
    if (!selectedClienteId) return;

    async function loadInventoryData() {
      setLoadingStock(true);
      setError(null);
      try {
        const rawInventory: RawStockRecord[] = await fetchInventory(selectedClienteId, token);

        if (!Array.isArray(rawInventory)) {
          setRawStock([]);
          return;
        }

        setRawStock(rawInventory);
      } catch (err: any) {
        console.error("[DEBUG] Inventory fetch error:", err);
        setError("Error al consultar el inventario del cliente.");
      } finally {
        setLoadingStock(false);
      }
    }

    if (token) {
      loadInventoryData();
    }
  }, [selectedClienteId, token]);

  const uniqueUbicaciones = useMemo(() => {
    const set = new Set<string>();
    rawStock.forEach((item) => {
      if (item.codigoUbicacion) set.add(item.codigoUbicacion);
    });
    return Array.from(set).sort();
  }, [rawStock]);

  const uniqueStatuses = useMemo(() => {
    const set = new Set<string>();
    rawStock.forEach((item) => {
      const qty = Number(item.cantidadActual) || 0;
      const rawStatus = item.status ?? item.Status ?? item.estado;
      const st = rawStatus !== undefined && rawStatus !== null 
        ? formatEstadoLote(rawStatus) 
        : (qty > 0 ? "Disponible" : "Agotado");
      if (st) set.add(st);
    });
    return Array.from(set).sort();
  }, [rawStock]);

  const filteredGroupedSkus = useMemo(() => {
    const filteredRecords = rawStock.filter((item) => {
      const qty = Number(item.cantidadActual) || 0;
      const rawStatus = item.status ?? item.Status ?? item.estado;
      const itemStatus = rawStatus !== undefined && rawStatus !== null 
        ? formatEstadoLote(rawStatus) 
        : (qty > 0 ? "Disponible" : "Agotado");
      const itemUbicacion = item.codigoUbicacion || "N/A";

      const matchesStatus =
        selectedStatus === "TODOS" || itemStatus.toLowerCase() === selectedStatus.toLowerCase();
      const matchesUbicacion =
        selectedUbicacion === "TODAS" || itemUbicacion === selectedUbicacion;

      return matchesStatus && matchesUbicacion;
    });

    return groupStockBySku(filteredRecords);
  }, [rawStock, selectedStatus, selectedUbicacion]);

  const toggleExpand = (idSku: number, e?: React.MouseEvent) => {
    if (e) {
      e.stopPropagation();
    }
    setExpandedSkus((prev) => ({ ...prev, [idSku]: !prev[idSku] }));
  };

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return "N/A";
    const dateObj = new Date(dateStr);
    return !isNaN(dateObj.getTime()) ? dateObj.toLocaleString("es-CR") : dateStr;
  };

  const renderStatusBadge = (statusVal: unknown) => {
    const label = formatEstadoLote(statusVal);
    switch (label.toLowerCase()) {
      case "disponible":
        return (
          <span className="inline-block px-2 py-0.5 text-xs rounded-full bg-emerald-500/10 text-emerald-400 border border-emerald-500/30 font-sans font-medium">
            Disponible
          </span>
        );
      case "vencido":
        return (
          <span className="inline-block px-2 py-0.5 text-xs rounded-full bg-rose-500/10 text-rose-400 border border-rose-500/30 font-sans font-medium">
            Vencido
          </span>
        );
      case "descartado":
        return (
          <span className="inline-block px-2 py-0.5 text-xs rounded-full bg-amber-500/10 text-amber-400 border border-amber-500/30 font-sans font-medium">
            Descartado
          </span>
        );
      default:
        return (
          <span className="inline-block px-2 py-0.5 text-xs rounded-full bg-slate-500/10 text-slate-400 border border-slate-500/30 font-sans font-medium">
            {label}
          </span>
        );
    }
  };

  return (
    <section className="bg-[#10151D] border border-[#4D5660] rounded-xl overflow-hidden shadow-xl">
      <div className="px-6 py-4 border-b border-[#4D5660] flex flex-wrap items-center justify-between gap-4">
        <div className="flex items-center gap-4 flex-wrap">
          <h2 className="text-lg font-semibold text-[#FFFFFF] flex items-center gap-2">
            <Layers className="w-5 h-5 text-[#EEB134]" /> Inventario
          </h2>

          <div className="flex items-center gap-2 bg-[#10151D] border border-[#4D5660] rounded-lg px-3 py-1.5">
            <Building2 className="w-4 h-4 text-[#EEB134]" />
            <select
              value={selectedClienteId}
              onChange={(e) => setSelectedClienteId(e.target.value ? Number(e.target.value) : "")}
              disabled={loadingClientes}
              className="bg-transparent text-sm text-[#FFFFFF] focus:outline-none cursor-pointer"
            >
              {loadingClientes ? (
                <option value="">Cargando clientes...</option>
              ) : (
                clientes.map((c: any) => {
                  const id = c.idCliente ?? c.IdCliente ?? c.id_cliente;
                  const name = c.nombreEmpresa ?? c.NombreEmpresa ?? c.nombre ?? c.Nombre;
                  const jur = c.identificacionJuridica ?? c.IdentificacionJuridica;
                  return (
                    <option key={id} value={id} className="bg-[#10151D] text-[#FFFFFF]">
                      {name} {jur ? `(${jur})` : ""}
                    </option>
                  );
                })
              )}
            </select>
          </div>
        </div>

        <div className="flex items-center gap-3 flex-wrap">
          <div className="flex items-center gap-2 bg-[#10151D] border border-[#4D5660] rounded-lg px-3 py-1.5">
            <Filter className="w-4 h-4 text-[#EEB134]" />
            <span className="text-xs text-[#CEDCE4]">Ubicación:</span>
            <select
              value={selectedUbicacion}
              onChange={(e) => setSelectedUbicacion(e.target.value)}
              className="bg-transparent text-xs text-[#FFFFFF] focus:outline-none cursor-pointer"
            >
              <option value="TODAS" className="bg-[#10151D] text-[#FFFFFF]">Todas</option>
              {uniqueUbicaciones.map((u) => (
                <option key={u} value={u} className="bg-[#10151D] text-[#FFFFFF]">
                  {u}
                </option>
              ))}
            </select>
          </div>

          <div className="flex items-center gap-2 bg-[#10151D] border border-[#4D5660] rounded-lg px-3 py-1.5">
            <Filter className="w-4 h-4 text-[#EEB134]" />
            <span className="text-xs text-[#CEDCE4]">Estado:</span>
            <select
              value={selectedStatus}
              onChange={(e) => setSelectedStatus(e.target.value)}
              className="bg-transparent text-xs text-[#FFFFFF] focus:outline-none cursor-pointer"
            >
              <option value="TODOS" className="bg-[#10151D] text-[#FFFFFF]">Todos</option>
              {uniqueStatuses.map((st) => (
                <option key={st} value={st} className="bg-[#10151D] text-[#FFFFFF]">
                  {st}
                </option>
              ))}
            </select>
          </div>

          {loadingStock && (
            <span className="flex items-center gap-2 text-xs text-[#EEB134]">
              <Loader2 className="w-4 h-4 animate-spin" /> Cargando Inventario...
            </span>
          )}
        </div>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-[#10151D] border-b border-[#4D5660] text-xs uppercase text-[#CEDCE4]">
              <th className="py-3 px-4 w-12 text-center"></th>
              <th className="py-3 px-4 font-semibold">Código SKU / Lote</th>
              <th className="py-3 px-4 font-semibold text-center w-32">Estado</th>
              <th className="py-3 px-4 font-semibold">Nombre SKU</th>
              <th className="py-3 px-4 font-semibold">Ubicación</th>
              <th className="py-3 px-4 font-semibold text-right">Cantidad Actual</th>
              <th className="py-3 px-4 font-semibold">Fecha de Vencimiento</th>
              <th className="py-3 px-4 font-semibold">Última Modificación</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-[#4D5660]/50 text-sm">
            {error ? (
              <tr>
                <td colSpan={8} className="text-center py-8 text-rose-400">
                  <AlertTriangle className="w-6 h-6 mx-auto mb-1" />
                  {error}
                </td>
              </tr>
            ) : filteredGroupedSkus.length === 0 && !loadingStock ? (
              <tr>
                <td colSpan={8} className="text-center py-8 text-[#CEDCE4]">
                  <Package className="w-6 h-6 mx-auto mb-1 text-[#4D5660]" />
                  Sin registros de inventario para este cliente.
                </td>
              </tr>
            ) : (
              filteredGroupedSkus.map((sku) => {
                const isExpanded = !!expandedSkus[sku.idSku];
                const safeTotalQty = Number(sku.totalCantidad) || 0;
                const locationCount = sku.locations?.length || 0;
                const primaryStatus = sku.locations?.[0]?.status || (safeTotalQty > 0 ? "Disponible" : "Agotado");

                return (
                  <React.Fragment key={`sku-group-${sku.idSku}`}>
                    <tr
                      onClick={(e) => toggleExpand(sku.idSku, e)}
                      className="hover:bg-[#4D5660]/20 cursor-pointer bg-[#10151D] select-none font-medium"
                    >
                      <td className="py-3 px-4 text-center">
                        <button
                          type="button"
                          onClick={(e) => toggleExpand(sku.idSku, e)}
                          className="focus:outline-none p-1"
                        >
                          {isExpanded ? (
                            <ChevronDown className="w-4 h-4 text-[#EEB134]" />
                          ) : (
                            <ChevronRight className="w-4 h-4 text-[#CEDCE4]" />
                          )}
                        </button>
                      </td>
                      <td className="py-3 px-4 font-mono font-bold text-[#EEB134]">
                        {sku.codigoSku}
                      </td>
                      <td className="py-3 px-4 text-center">
                        {renderStatusBadge(primaryStatus)}
                      </td>
                      <td className="py-3 px-4 text-[#FFFFFF] font-semibold">{sku.nombreSku}</td>
                      <td className="py-3 px-4 text-xs text-[#CEDCE4] italic">
                        {locationCount} Ubicación(es)
                      </td>
                      <td className="py-3 px-4 text-right font-bold text-[#FFFFFF] font-mono">
                        {safeTotalQty.toLocaleString()}
                      </td>
                      <td className="py-3 px-4 text-xs text-[#CEDCE4] italic">&mdash;</td>
                      <td className="py-3 px-4 text-xs text-[#CEDCE4] italic">&mdash;</td>
                    </tr>

                    {isExpanded &&
                      sku.locations.map((loc) => (
                        <tr
                          key={`stock-row-${loc.idStock}`}
                          className="bg-[#10151D]/60 border-l-2 border-l-[#EEB134] text-xs font-mono"
                        >
                          <td></td>
                          <td className="py-2.5 px-4 text-[#CEDCE4] font-bold">
                            ↳ {loc.numeroLote}
                          </td>
                          <td className="py-2.5 px-4 text-center">
                            {renderStatusBadge(loc.status)}
                          </td>
                          <td className="py-2.5 px-4 text-[#CEDCE4]/50 italic">&mdash;</td>
                          <td className="py-2.5 px-4 text-[#EEB134] font-bold">{loc.codigoUbicacion}</td>
                          <td className="py-2.5 px-4 text-right text-[#FFFFFF] font-bold">
                            {loc.cantidadActual.toLocaleString()}
                          </td>
                          <td className="py-2.5 px-4 text-[#CEDCE4]">{formatDate(loc.fechaVencimiento)}</td>
                          <td className="py-2.5 px-4 text-[#CEDCE4]">{formatDate(loc.fechaActualizacion)}</td>
                        </tr>
                      ))}
                  </React.Fragment>
                );
              })
            )}
          </tbody>
        </table>
      </div>
    </section>
  );
};