<template>
    <FlowComponent 
        :stepOrder="routeStepOrder"
        :phase="routePhase"
        :workflowId="routeWorkflowId"
    />
</template>

<script>
    import FlowComponent from '@/components/flow/FlowComponent.vue';
    import { useRoute } from 'vue-router';
    export default {
        name: "NewFlow",
        components: {
            FlowComponent
        },
        data() {
            return {
                persistData: {},
            };
        },
        computed: {
            routeStepOrder() {
                return parseInt(this.$route.params.stepOrder);
            },
            routePhase() {
                return parseInt(this.$route.params.phase);
            },
            routeWorkflowId() {
                return this.$route.params.workflowId ? parseInt(this.$route.params.workflowId) : null;
            }
        },
    }
</script>