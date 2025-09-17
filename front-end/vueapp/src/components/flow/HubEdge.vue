<template>
<BaseEdge :path="path[0]" />

  <EdgeLabelRenderer>
    <div
      :style="{
        pointerEvents: 'all',
        position: 'absolute',
        transform: `translate(-50%, -50%) translate(${path[1]}px,${path[2]}px)`,
      }"
      class="nodrag nopan"
    >
      {{ edge.data.hello }}
    </div>
  </EdgeLabelRenderer>
</template>

<script>
import { BaseEdge, EdgeLabelRenderer, getBezierPath } from '@vue-flow/core'
import LucideIcon from '@/components/global/LucideIcon.vue'

export default {
  name: 'HubEdge',
  inheritAttrs: false,

  props: {
    sourceX: {
      type: Number,
      required: true,
    },
    sourceY: {
      type: Number,
      required: true,
    },
    targetX: {
      type: Number,
      required: true,
    },
    targetY: {
      type: Number,
      required: true,
    },
    sourcePosition: {
      type: String,
      required: true,
    },
    targetPosition: {
      type: String,
      required: true,
    },
    edge: {
      type: Object,
      required: true,
    },
  },
  components: {
    EdgeLabelRenderer, BaseEdge
  },
  computed: {
    path() {
      return getBezierPath({
        sourceX: this.sourceX,
        sourceY: this.sourceY,
        targetX: this.targetX,
        targetY: this.targetY,
        sourcePosition: this.sourcePosition,
        targetPosition: this.targetPosition,
      })
    },
  },
}
</script>
