#!/usr/bin/env node

import fs from "node:fs";

const API_BASE_URL =
  process.env.API_BASE_URL ||
  "http://127.0.0.1:5000/api/v1";

const TOKEN = process.env.ADMIN_ACCESS_TOKEN;

if (!TOKEN) {
  throw new Error("Thiếu ADMIN_ACCESS_TOKEN");
}

const mapping = JSON.parse(
  fs.readFileSync("./civicpulse-boundary-mapping.json", "utf8")
);

const ONLY_AREA_IDS = new Set([84, 118, 78, 89, 120, 98]);

const originalMapping = mapping;
const mappingFiltered = originalMapping.filter(
  (item) => ONLY_AREA_IDS.has(Number(item.areaId)),
);

console.log(
  `Filtered mapping: ${mappingFiltered.length}/${originalMapping.length}`,
);

if (mappingFiltered.length !== ONLY_AREA_IDS.size) {
  throw new Error(
    `Thiếu area trong mapping: expected ${ONLY_AREA_IDS.size}, got ${mappingFiltered.length}`,
  );
}


function pointKey(p) {
  return `${p[0]},${p[1]}`;
}

function samePoint(a, b) {
  return a[0] === b[0] && a[1] === b[1];
}

/*
 * geometry trong mapping là các đoạn:
 *
 * [
 *   [point, point, ...],
 *   [point, point, ...],
 *   ...
 * ]
 *
 * Ghép các đoạn có chung endpoint thành các closed rings.
 */
function buildRings(parts) {
  const segments = (parts || [])
    .filter(
      p =>
        Array.isArray(p) &&
        p.length >= 2 &&
        Array.isArray(p[0]) &&
        Array.isArray(p[p.length - 1])
    )
    .map(p => p.map(x => [Number(x[0]), Number(x[1])]));

  const unused = new Set(segments.map((_, i) => i));
  const rings = [];

  while (unused.size) {
    const firstIndex = unused.values().next().value;
    unused.delete(firstIndex);

    let ring = [...segments[firstIndex]];

    const start = [...ring[0]];
    let current = [...ring[ring.length - 1]];

    while (!samePoint(current, start)) {
      let found = -1;
      let reversed = false;

      for (const i of unused) {
        const seg = segments[i];

        if (samePoint(seg[0], current)) {
          found = i;
          reversed = false;
          break;
        }

        if (samePoint(seg[seg.length - 1], current)) {
          found = i;
          reversed = true;
          break;
        }
      }

      if (found === -1) {
        throw new Error(
          `Không thể nối ring tại endpoint ${current.join(",")}`
        );
      }

      unused.delete(found);

      let seg = segments[found];

      if (reversed) {
        seg = [...seg].reverse();
      }

      // bỏ điểm đầu vì trùng current
      ring.push(...seg.slice(1));
      current = [...ring[ring.length - 1]];
    }

    // GeoJSON LinearRing phải đóng
    if (!samePoint(ring[0], ring[ring.length - 1])) {
      ring.push([...ring[0]]);
    }

    if (ring.length < 4) {
      throw new Error("Ring có ít hơn 4 điểm");
    }

    rings.push(ring);
  }

  return rings;
}

function areaToGeoJSON(area) {
  const rings = buildRings(area.geometry);

  if (!rings.length) {
    throw new Error("Không có ring");
  }

  if (rings.length === 1) {
    return {
      type: "Polygon",
      coordinates: rings,
    };
  }

  return {
    type: "MultiPolygon",
    coordinates: rings.map(ring => [ring]),
  };
}

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      Authorization: `Bearer ${TOKEN}`,
      "Content-Type": "application/json",
      ...(options.headers || {}),
    },
  });

  const text = await response.text();

  if (!response.ok) {
    throw new Error(
      `${response.status} ${response.statusText}: ${text.slice(0, 1000)}`
    );
  }

  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

async function main() {
  console.log(`Mapping areas: ${mappingFiltered.length}`);

  let ok = 0;
  let failed = 0;

  for (let i = 0; i < mappingFiltered.length; i++) {
    const area = mappingFiltered[i];

    try {
      const geometry = areaToGeoJSON(area);

      console.log(
        `[${i + 1}/${mappingFiltered.length}] ${area.areaName} ` +
        `→ ${geometry.type}, ${area.geometry.length} parts`
      );

      await request(`/admin/areas/${area.areaId}/boundary`, {
        method: "PUT",
        body: JSON.stringify({
          geoJson: JSON.stringify(geometry),
        }),
      });

      console.log(`  ✓ SAVED`);
      ok++;
    } catch (error) {
      console.error(`  ✗ FAILED: ${error.message}`);
      failed++;
    }
  }

  console.log();
  console.log(`DONE: ok=${ok}, failed=${failed}`);
}

main().catch(error => {
  console.error(error);
  process.exit(1);
});
