<template>
    <FlowComponent
        :isEdit="true"
        :id="routeId"
        :stepId="routeStepId"
        :stepOrder="routeStepOrder"
        :phase="routePhase"
        :workflowId="routeWorkflowId"
    />
</template>

<script>
    import FlowComponent from '@/components/flow/FlowComponent.vue';
    export default {
        name: "EditFlow",
        components: {
            FlowComponent
        },
        computed: {
            routeId() {
                return parseInt(this.$route.params.id);
            },
            routeStepId() {
                return parseInt(this.$route.params.stepId);
            },
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