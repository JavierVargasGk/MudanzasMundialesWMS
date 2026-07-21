import { z } from 'zod';


export const loginSchema = z.object({
  Correo: z.string().min(1, 'El correo es requerido').email('Correo electrónico inválido'),
  Contrasena: z.string().min(1, 'La contraseña es requerida'),
});

export const adjustmentSchema = z.object({
  sku_id: z.string().uuid(),
  cantidad: z.number().int().min(1, "Quantity must be positive"),
  motivo: z.string().min(5),
});

export const entrySchema = z.object({
  sku_id: z.string().uuid(),
  cantidad: z.number().int().gt(0),
  ubicacion_id: z.string().uuid(),
});


export const inventoryItemSchema = z.object({
  id: z.string().uuid(),
  sku_code: z.string(),
  sku_name: z.string(),
  stock_actual: z.number().int().nonnegative(),
  ubicacion_codigo: z.string(),
  fecha_actualizacion: z.string().datetime(), 
});

export type LoginFormData = z.infer<typeof loginSchema>;
export type AdjustmentFormData = z.infer<typeof adjustmentSchema>;
export type EntryFormData = z.infer<typeof entrySchema>;
export type InventoryItem = z.infer<typeof inventoryItemSchema>;