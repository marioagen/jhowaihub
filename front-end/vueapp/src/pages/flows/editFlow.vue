<template>
    <FlowComponent
        :isEdit="true"
        :id="$route.params.id"
        :stepId="$route.params.stepId"
        :stepOrder="$route.params.stepOrder"
    />
</template>

<script>
    import FlowComponent from '@/components/flow/FlowComponent.vue';
    export default {
        name: "EditFlow",
        components: {
            FlowComponent
        },
    }
</script>