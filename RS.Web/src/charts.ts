// The parts of ECharts the Analiz page uses (R38, R36), installed once when this module is first imported, so they are
// in place before any chart or map is set up. Imported only by the Analiz page's components, so ECharts stays out of
// the other pages' bundle.
import { registerMap, use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { BarChart, LineChart, MapChart } from 'echarts/charts'
import {
  GraphicComponent,
  GridComponent,
  LegendComponent,
  TitleComponent,
  ToolboxComponent,
  TooltipComponent,
  VisualMapComponent,
} from 'echarts/components'
// Province borders: Natural Earth 1:10m admin-1 (public domain, v5.1.2), simplified with mapshaper; each shape is named
// by its plate code, which is also the app's province id.
import provinces from '@/assets/turkey-provinces.json'

use([
  CanvasRenderer,
  BarChart,
  LineChart,
  MapChart,
  GridComponent,
  LegendComponent,
  TooltipComponent,
  ToolboxComponent,
  TitleComponent,
  GraphicComponent,
  VisualMapComponent,
])
registerMap('turkey', provinces as Parameters<typeof registerMap>[1])

/** The province shapes, for the map's data (one item per province). */
export const provinceCodes: number[] = (provinces as { features: { properties: { name: string } }[] }).features.map((f) =>
  Number(f.properties.name),
)
