<template>
    <BaseEdge :path="path[0]" />
    <EdgeLabelRenderer>
        <div :style="{
            pointerEvents: 'all',
            position: 'absolute',
            transform: `translate(-50%, -50%) translate(${path[1]}px,${path[2]}px)`,
        }" class="nodrag nopan">
            <LucideIcon :icon="'CircleX'" class="delete-edge" :size="16" @click="$emit('deleteEdge', data.id)" />
        </div>
    </EdgeLabelRenderer>
</template>

<script>
import { BaseEdge, EdgeLabelRenderer, getSmoothStepPath } from '@vue-flow/core'
import LucideIcon from '@/components/global/LucideIcon.vue'

export default {
    name: 'SpecialEdge',
    inheritAttrs: false,
    components: {
        EdgeLabelRenderer, BaseEdge, LucideIcon
    },
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
        data: {
            type: Object,
            required: true,
        },
    },
    computed: {
        path() {
            return getSmoothStepPath({
                sourceX: this.sourceX,
                sourceY: this.sourceY,
                targetX: this.targetX,
                targetY: this.targetY,
                sourcePosition: this.sourcePosition,
                targetPosition: this.targetPosition,
            })
        }
    },
}
</script>
<style>
.delete-edge {
    margin-top: -3px;
    cursor: pointer;
    color: #6c757d;
}

.delete-edge:hover {
    color: #a71d2a;
}
</style>
