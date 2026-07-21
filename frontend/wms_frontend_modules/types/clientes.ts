
export interface Cliente {
  idCliente: number;
  nombreEmpresa: string;
  identificacionJuridica?: string;
  contactoNombre?: string;
  contactoTelefono?: string;
  contactoCorreo?: string;
  activo: boolean;
  fechaRegistro: string;
}