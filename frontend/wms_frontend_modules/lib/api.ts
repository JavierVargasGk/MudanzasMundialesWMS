// @/lib/api.ts

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL ||
  process.env.API_BASE_URL ||
  "http://localhost:5080";

export async function apiFetch(
  endpoint: string,
  options: RequestInit = {},
  token?: string
) {
  const defaultHeaders: Record<string, string> = {
    "Content-Type": "application/json",
  };

  const authToken =
    token ||
    (typeof window !== "undefined"
      ? localStorage.getItem("accessToken")
      : undefined);

  if (authToken) {
    defaultHeaders["Authorization"] = `Bearer ${authToken}`;
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers: {
      ...defaultHeaders,
      ...options.headers,
    },
  });

  return response;
}

export async function fetchClientes(token?: string) {
  const response = await apiFetch("/api/clientes", { method: "GET" }, token);
  if (!response.ok) {
    throw new Error(`HTTP ${response.status}: Error al obtener clientes.`);
  }
  return response.json();
}

export async function fetchInventory(idCliente: number | string, token?: string) {
  const response = await apiFetch(
    `/api/clientes/${idCliente}/inventario/stock`,
    {
      method: "GET",
      cache: "no-store",
    },
    token
  );

  if (!response.ok) {
    throw new Error(`HTTP ${response.status}: Error al obtener el inventario.`);
  }

  return response.json();
}

export async function fetchSkus(clienteId: number | string, skuId: number | string, token?: string) {
  const response = await apiFetch(
    `/api/clientes/${clienteId}/skus/${skuId}`,
    { method: "GET" },
    token
  );

  if (!response.ok) {
    throw new Error(`HTTP ${response.status}: Error al obtener el SKU ${skuId}.`);
  }

  return response.json();
}