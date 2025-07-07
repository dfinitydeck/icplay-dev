use crate::cycles_handler::CycleTransferResult;
use candid::{export_service, Principal};
use canister_types::{indexer::CanisterArgs, message::Message};
use ic_cdk::query;

/// Generates the Candid interface for this canister
/// 
/// This function exports the service interface in Candid format, which is used
/// for type-safe communication between canisters and clients.
/// 
/// # Returns
/// A string containing the Candid interface definition
#[query(name = "__get_candid_interface_tmp_hack")]
fn generate_candid_interface() -> String {
    export_service!();
    __export_service()
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::env;
    use std::fs;
    use std::path::PathBuf;

    /// Test function to save the generated Candid interface to a file
    /// 
    /// This test ensures that the Candid interface can be generated and
    /// saves it to a file for inspection and documentation purposes.
    /// 
    /// # Panics
    /// Panics if the current directory cannot be determined or if the file
    /// cannot be written to disk
    #[test]
    fn save_candid_interface() {
        // Get the current directory for output
        let output_dir = env::current_dir()
            .expect("Failed to get current directory");
        
        // Generate the Candid interface
        let candid_interface = generate_candid_interface();
        
        // Create the output file path
        let output_file = output_dir.join("canister_indexer.did");
        
        // Write the interface to file
        fs::write(&output_file, candid_interface)
            .expect("Failed to write Candid interface to file");
        
        // Verify the file was created successfully
        assert!(output_file.exists(), "Candid interface file was not created");
    }
}
