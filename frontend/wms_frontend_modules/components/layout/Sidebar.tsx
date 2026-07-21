"use client";

import React, { useState } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import {
  LayoutDashboard,
  Package,
  ArrowDownLeft,
  ArrowUpRight,
  ClipboardCheck,
  FileSpreadsheet,
  Users,
  LogOut,
  ChevronLeft,
  ChevronRight,
  Shield,
  Layers,
} from "lucide-react";
import { UserRole } from "@/types/auth";

interface Props {
  role: UserRole;
  userName?: string;
  userEmail?: string;
}

interface NavItem {
  label: string;
  href: string;
  icon: React.ElementType;
  roles: UserRole[];
}

const NAV_ITEMS: NavItem[] = [
  {
    label: "Dashboard",
    href: "/dashboard",
    icon: LayoutDashboard,
    roles: ["Administrador", "Analista", "Operario"],
  },
  {
    label: "Inventario",
    href: "/inventory",
    icon: Package,
    roles: ["Administrador", "Analista", "Operario"],
  },
  {
    label: "Entradas",
    href: "/entradas",
    icon: ArrowDownLeft,
    roles: ["Administrador", "Operario"],
  },
  {
    label: "Despachos",
    href: "/despachos",
    icon: ArrowUpRight,
    roles: ["Administrador", "Operario"],
  },
  {
    label: "Ajustes",
    href: "/ajustes",
    icon: ClipboardCheck,
    roles: ["Administrador"],
  },
  {
    label: "Reportes",
    href: "/reportes",
    icon: FileSpreadsheet,
    roles: ["Administrador", "Analista"],
  },
  {
    label: "Clientes",
    href: "/clientes",
    icon: Users,
    roles: ["Administrador"],
  },
];

export const Sidebar: React.FC<Props> = ({
  role,
  userName = "Usuario",
  userEmail = "usuario@empresa.com",
}) => {
  const [collapsed, setCollapsed] = useState<boolean>(false);
  const pathname = usePathname();
  const router = useRouter();

  // Hide sidebar completely on login page
  if (pathname === "/login") return null;

  const handleLogout = async () => {
    document.cookie = "accessToken=; path=/; expires=Thu, 01 Jan 1970 00:00:01 GMT;";
    if (typeof window !== "undefined") {
      localStorage.removeItem("accessToken");
    }
    router.push("/login");
  };

  const filteredItems = NAV_ITEMS.filter((item) => item.roles.includes(role));

  return (
    <aside
      className={`relative flex flex-col bg-[#10151D] border-r border-[#4D5660] text-white transition-all duration-300 min-h-screen z-30 ${
        collapsed ? "w-20" : "w-64"
      }`}
    >
      <div className="flex items-center justify-between h-16 px-4 border-b border-[#4D5660]">
        {!collapsed && (
          <div className="flex items-center gap-2.5 overflow-hidden">
            <div className="p-1.5 bg-[#EEB134] text-[#10151D] rounded-lg shrink-0">
              <Layers className="w-5 h-5 font-bold" />
            </div>
            <div className="flex flex-col leading-none">
              <span className="font-bold text-sm tracking-wide text-white">
                MUDANZAS
              </span>
              <span className="text-[10px] text-[#EEB134] font-semibold tracking-wider uppercase">
                Logística
              </span>
            </div>
          </div>
        )}

        {collapsed && (
          <div className="mx-auto p-1.5 bg-[#EEB134] text-[#10151D] rounded-lg">
            <Layers className="w-5 h-5" />
          </div>
        )}

        <button
          type="button"
          onClick={() => setCollapsed(!collapsed)}
          className={`p-1.5 rounded-lg text-[#CEDCE4] hover:bg-[#4D5660]/30 hover:text-white transition-colors ${
            collapsed ? "mt-2 mx-auto" : ""
          }`}
          title={collapsed ? "Expandir" : "Colapsar"}
        >
          {collapsed ? <ChevronRight className="w-5 h-5" /> : <ChevronLeft className="w-5 h-5" />}
        </button>
      </div>

      {!collapsed && (
        <div className="px-4 py-3 border-b border-[#4D5660]/50 bg-[#10151D]/50">
          <div className="flex items-center gap-2 px-2.5 py-1 rounded-md bg-[#4D5660]/20 border border-[#4D5660]/40 text-xs">
            <Shield className="w-3.5 h-3.5 text-[#EEB134]" />
            <span className="text-[#CEDCE4]">Rol:</span>
            <span className="font-semibold text-white ml-auto">{role}</span>
          </div>
        </div>
      )}
      <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        {filteredItems.map((item) => {
          const isActive = pathname === item.href || pathname?.startsWith(`${item.href}/`);
          const Icon = item.icon;

          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all group relative ${
                isActive
                  ? "bg-[#EEB134] text-[#10151D] font-semibold shadow-md"
                  : "text-[#CEDCE4] hover:bg-[#4D5660]/30 hover:text-white"
              }`}
              title={collapsed ? item.label : undefined}
            >
              <Icon
                className={`w-5 h-5 shrink-0 ${
                  isActive ? "text-[#10151D]" : "text-[#CEDCE4] group-hover:text-[#EEB134]"
                }`}
              />

              {!collapsed && <span>{item.label}</span>}

              {collapsed && (
                <div className="absolute left-full ml-2 px-2.5 py-1 bg-[#10151D] text-white text-xs rounded border border-[#4D5660] shadow-lg opacity-0 pointer-events-none group-hover:opacity-100 transition-opacity z-50 whitespace-nowrap">
                  {item.label}
                </div>
              )}
            </Link>
          );
        })}
      </nav>

      <div className="p-3 border-t border-[#4D5660] bg-[#10151D]">
        {!collapsed && (
          <div className="flex items-center justify-between mb-2 px-2">
            <div className="truncate pr-2">
              <p className="text-xs font-semibold text-white truncate">{userName}</p>
              <p className="text-[10px] text-[#CEDCE4] truncate">{userEmail}</p>
            </div>
          </div>
        )}

        <button
          type="button"
          onClick={handleLogout}
          className={`w-full flex items-center justify-center gap-2 px-3 py-2 rounded-lg text-xs font-medium text-rose-400 hover:bg-rose-500/10 hover:text-rose-300 border border-transparent hover:border-rose-500/30 transition-all ${
            collapsed ? "px-0" : ""
          }`}
          title="Cerrar sesión"
        >
          <LogOut className="w-4 h-4 shrink-0" />
          {!collapsed && <span>Cerrar Sesión</span>}
        </button>
      </div>
    </aside>
  );
};