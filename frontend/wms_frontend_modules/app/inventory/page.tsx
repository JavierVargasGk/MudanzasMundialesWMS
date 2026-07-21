import { cookies } from "next/headers";
import { InventoryTableTree } from "@/components/inventory/InventoryTableTree";

export default async function InventoryPage() {
  const cookieStore = await cookies();
  const token = cookieStore.get("accessToken")?.value ?? "";

  return <InventoryTableTree token={token} />;
}