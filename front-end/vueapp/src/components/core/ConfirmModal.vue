<template>
    <div
        class="modal fade"
        tabindex="-1"
        aria-hidden="true"
        ref="modalEl"
        :aria-labelledby="`${id}-label`"
        :id="id"
    >
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 
                        class="modal-title" 
                        :id="`${id}-label`"
                    >
                        {{ title }}
                    </h5>
                    <button 
                        type="button" 
                        class="btn-close" 
                        data-bs-dismiss="modal" 
                        aria-label="Close" 
                    />
                </div>
                
                <div class="modal-body">
                    {{ message }}
                </div>

                <div class="justify-content-center mx-4 my-4">
                    <button
                        type="button"
                        class="btn btn-secondary mx-4"
                        data-bs-dismiss="modal"
                        @click="$emit('cancel')"
                    >
                        Cancel
                    </button>
                    <button
                        type="button"
                        class="btn btn-danger mx-4"
                        data-bs-dismiss="modal"
                        @click="$emit('confirm')"
                    >
                        Confirm
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: "ConfirmDialog",
        props: {
            id: { 
                type: String, 
                required: true 
            },
            title: { 
                type: String, 
                required: false,
                default: "Are you sure?"
            },
            message: { 
                type: String, 
                required: false,
                default: "Are you sure of this action?"
            },
        },
    }
</script>