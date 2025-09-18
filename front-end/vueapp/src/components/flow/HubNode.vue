<template>
  <div class="vue-flow__node-default item-box shadow-sm">
    <Handle v-if="!node.data.isStartNode" type="target" position="left" class="hub-handle" />
    <div class="item-left me-2">
      <LucideIcon :icon="node.data.icon" :color="node.data.color" />
      <h5 class="mb-0">{{ node.label }}</h5>
    </div>
    <div class="item-right" v-if="!node.data.isStartNode">
      <LucideIcon :icon="'Settings'" class="settings" :size="16" @click="$emit('openNodeConfig', node.id)" />
      <LucideIcon :icon="'X'" class="delete" :size="16" @click="$emit('deleteNode', node.id)" />
    </div>
    <Handle type="source" position="right" class="hub-handle" />
  </div>
</template>

<script>
import { Handle } from '@vue-flow/core'
import LucideIcon from '@/components/global/LucideIcon.vue'

export default {
  name: 'HubNode',
  props: {
    node: {
      type: Object,
      required: true
    }
  },
  components: {
    Handle,
    LucideIcon
  }
}
</script>
<style>
.item-box {
  display: flex;
  align-items: center;
  justify-content: space-between;
  border: 1px solid #dee2e6 !important;
  border-radius: 8px !important;
  padding: 0.5rem 1rem;
  background-color: #fff;
  white-space: nowrap;
  width: auto !important;
}

.item-left {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.item-right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.settings,
.delete {
  cursor: pointer;
  color: #6c757d;
}

.settings:hover {
  color: #0d6efd;
}

.delete:hover {
  color: #dc3545;
}

.hub-handle {
  width: 14px !important;
  height: 14px !important;
  background: var(--color-bg-btn-primary) !important;
  border: 1px solid #fff;
  border-radius: 100%;
  pointer-events: all;
  cursor: crosshair;
}
</style>