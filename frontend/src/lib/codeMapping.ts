interface CodedRecord {
  id: string;
  code: string;
}

/**
 * Convierte códigos (como los expone la API en RoleDto.permissions o UserDto.roles) a los IDs que esperan
 * los endpoints de asignación. Devuelve null si el catálogo aún no contiene todos los códigos, para no
 * presentar una selección incompleta que reemplazaría la asignación real al guardar.
 */
export function codesToIds(codes: unknown, catalog: CodedRecord[]): string[] | null {
  if (!Array.isArray(codes)) return [];
  const idsByCode = new Map(catalog.map((item) => [item.code.toUpperCase(), item.id]));
  const ids: string[] = [];
  for (const code of codes) {
    if (typeof code !== "string") continue;
    const id = idsByCode.get(code.toUpperCase());
    if (!id) return null;
    ids.push(id);
  }
  return ids;
}
